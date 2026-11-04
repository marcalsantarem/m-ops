import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { KanbanSquare, LogIn } from 'lucide-react'
import { ApiError, clearToken, projectsApi, setToken } from '../api'
import { useAuth } from '../context/authContext'

export default function LoginPage() {
  const { token, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from || '/projects'
  const [value, setValue] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  if (token && !busy) return <Navigate to={from} replace />

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const trimmed = value.trim()
    if (!trimmed) {
      setError('Informe o token.')
      return
    }
    setBusy(true)
    setError(null)
    setToken(trimmed)
    try {
      await projectsApi.list()
      login(trimmed)
      navigate(from, { replace: true })
    } catch (err) {
      clearToken()
      setError(
        err instanceof ApiError && err.status === 401
          ? 'Token inválido.'
          : `Não foi possível validar o token: ${err instanceof Error ? err.message : 'erro desconhecido'}`,
      )
    } finally {
      setBusy(false)
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center p-4">
      <form onSubmit={handleSubmit} className="w-full max-w-sm space-y-4 rounded-xl border border-zinc-800 bg-zinc-900 p-6">
        <div className="flex items-center gap-2 text-xl font-bold">
          <KanbanSquare className="h-7 w-7 text-sky-400" aria-hidden="true" />
          M-Ops
        </div>
        <label className="block">
          <span className="mb-1 block text-sm text-zinc-300">Token da API</span>
          <input
            type="password"
            autoComplete="off"
            autoFocus
            value={value}
            onChange={(e) => setValue(e.target.value)}
            className="w-full rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm focus:border-sky-500 focus:outline-none"
            placeholder="Cole o token aqui"
          />
        </label>
        {error && (
          <p className="text-sm text-red-400" role="alert">
            {error}
          </p>
        )}
        <button
          type="submit"
          disabled={busy}
          className="flex w-full items-center justify-center gap-2 rounded-md bg-sky-600 px-3 py-2 text-sm font-medium hover:bg-sky-500 disabled:opacity-60"
        >
          <LogIn className="h-4 w-4" aria-hidden="true" />
          {busy ? 'Validando...' : 'Entrar'}
        </button>
      </form>
    </main>
  )
}
