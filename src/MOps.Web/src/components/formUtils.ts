import { ApiError } from '../api'

export type FieldErrors = Record<string, string[]>

/** Normaliza as chaves de ApiError.errors (ex.: "Name", "$.name") para minúsculas. */
export function normalizeErrors(errors?: Record<string, string[]>): FieldErrors {
  const out: FieldErrors = {}
  for (const [key, messages] of Object.entries(errors ?? {})) {
    const k = key.replace(/^\$\./, '').toLowerCase()
    out[k] = [...(out[k] ?? []), ...messages]
  }
  return out
}

/** Converte um erro de salvamento em erros por campo + mensagem geral (campos fora de `known`). */
export function splitSaveError(err: unknown, known: string[]): { fields: FieldErrors; form: string | null } {
  if (err instanceof ApiError && err.errors) {
    const fields = normalizeErrors(err.errors)
    const other = Object.entries(fields)
      .filter(([k]) => !known.includes(k))
      .flatMap(([, v]) => v)
    return { fields, form: other.length ? other.join(' ') : null }
  }
  return { fields: {}, form: err instanceof Error ? err.message : 'Erro ao salvar.' }
}

export const inputClass =
  'w-full rounded-md border bg-zinc-950 px-3 py-2 text-sm focus:outline-none focus:border-sky-500'

export const selectClass =
  'rounded-md border border-zinc-700 bg-zinc-950 px-2 py-1.5 text-sm focus:border-sky-500 focus:outline-none'

export const borderFor = (errors: FieldErrors, field: string) => (errors[field] ? 'border-red-500' : 'border-zinc-700')
