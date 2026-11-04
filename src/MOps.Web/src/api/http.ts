import { getToken } from './auth'
import type { ProblemDetails } from './types'

const BASE_URL = (import.meta.env.VITE_API_URL ?? '').replace(/\/+$/, '')

export class ApiError extends Error {
  readonly status: number
  readonly title?: string
  readonly detail?: string
  readonly errors?: Record<string, string[]>
  readonly problem?: ProblemDetails

  constructor(status: number, problem?: ProblemDetails, fallback?: string) {
    super(problem?.detail ?? problem?.title ?? fallback ?? `Erro HTTP ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
    this.title = problem?.title
    this.detail = problem?.detail
    this.errors = problem?.errors
  }
}

let unauthorizedHandler: (() => void) | null = null

/** Registra o callback chamado em respostas 401. Retorna função que desfaz o registro. */
export function onUnauthorized(handler: (() => void) | null): () => void {
  unauthorizedHandler = handler
  return () => {
    if (unauthorizedHandler === handler) unauthorizedHandler = null
  }
}

export type QueryValue = string | number | boolean | null | undefined

export interface RequestOptions {
  body?: unknown
  query?: Record<string, QueryValue>
  signal?: AbortSignal
}

function buildUrl(path: string, query?: Record<string, QueryValue>): string {
  const url = `${BASE_URL}${path.startsWith('/') ? path : `/${path}`}`
  if (!query) return url
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') params.append(key, String(value))
  }
  const qs = params.toString()
  return qs ? `${url}?${qs}` : url
}

async function parseBody(res: Response): Promise<unknown> {
  if (res.status === 204 || res.status === 205) return undefined
  const text = await res.text()
  if (!text) return undefined
  if ((res.headers.get('content-type') ?? '').includes('json')) {
    try {
      return JSON.parse(text)
    } catch {
      return text
    }
  }
  return text
}

export async function request<T>(method: string, path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' }
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`
  const hasBody = options.body !== undefined
  if (hasBody) headers['Content-Type'] = 'application/json'

  const res = await fetch(buildUrl(path, options.query), {
    method,
    headers,
    body: hasBody ? JSON.stringify(options.body) : undefined,
    signal: options.signal,
  })

  const data = await parseBody(res)

  if (!res.ok) {
    if (res.status === 401) unauthorizedHandler?.()
    const problem = data && typeof data === 'object' ? (data as ProblemDetails) : undefined
    const fallback = typeof data === 'string' && data ? data : res.statusText || undefined
    throw new ApiError(res.status, problem, fallback)
  }

  return data as T
}

export const http = {
  get: <T>(path: string, options?: RequestOptions) => request<T>('GET', path, options),
  post: <T>(path: string, body?: unknown, options?: RequestOptions) => request<T>('POST', path, { ...options, body }),
  put: <T>(path: string, body?: unknown, options?: RequestOptions) => request<T>('PUT', path, { ...options, body }),
  patch: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>('PATCH', path, { ...options, body }),
  delete: <T = void>(path: string, options?: RequestOptions) => request<T>('DELETE', path, options),
}
