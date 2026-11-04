import { useState, type ReactNode } from 'react'
import { Link } from 'react-router'
import { AlertCircle, FolderKanban, Loader2, RefreshCw } from 'lucide-react'
import type { FieldErrors } from './formUtils'

export function Field({
  label,
  name,
  errors,
  children,
}: {
  label: string
  name: string
  errors: FieldErrors
  children: ReactNode
}) {
  const msgs = errors[name.toLowerCase()]
  return (
    <label className="block">
      <span className="mb-1 block text-sm text-zinc-300">{label}</span>
      {children}
      {msgs?.map((m) => (
        <span key={m} className="mt-1 block text-xs text-red-400">
          {m}
        </span>
      ))}
    </label>
  )
}

export function FormActions({ saving, onCancel, error }: { saving: boolean; onCancel: () => void; error: string | null }) {
  return (
    <>
      {error && (
        <p className="text-sm text-red-400" role="alert">
          {error}
        </p>
      )}
      <div className="flex justify-end gap-2">
        <button type="button" onClick={onCancel} className="rounded-md px-3 py-2 text-sm text-zinc-300 hover:bg-zinc-800">
          Cancelar
        </button>
        <button
          type="submit"
          disabled={saving}
          className="flex items-center gap-2 rounded-md bg-sky-600 px-3 py-2 text-sm font-medium hover:bg-sky-500 disabled:opacity-60"
        >
          {saving && <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />}
          Salvar
        </button>
      </div>
    </>
  )
}

/** Conteúdo de modal de confirmação de exclusão. */
export function ConfirmDelete({
  message,
  onConfirm,
  onDone,
  onCancel,
}: {
  message: ReactNode
  onConfirm: () => Promise<unknown>
  onDone: () => void
  onCancel: () => void
}) {
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function confirm() {
    setBusy(true)
    setError(null)
    try {
      await onConfirm()
      onDone()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao excluir.')
      setBusy(false)
    }
  }

  return (
    <div className="space-y-4">
      <div className="text-sm text-zinc-300">{message}</div>
      {error && (
        <p className="text-sm text-red-400" role="alert">
          {error}
        </p>
      )}
      <div className="flex justify-end gap-2">
        <button type="button" onClick={onCancel} className="rounded-md px-3 py-2 text-sm text-zinc-300 hover:bg-zinc-800">
          Cancelar
        </button>
        <button
          type="button"
          onClick={confirm}
          disabled={busy}
          className="flex items-center gap-2 rounded-md bg-red-600 px-3 py-2 text-sm font-medium hover:bg-red-500 disabled:opacity-60"
        >
          {busy && <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />}
          Excluir
        </button>
      </div>
    </div>
  )
}

export function LoadingState({ text }: { text: string }) {
  return (
    <div className="flex items-center gap-2 text-zinc-400">
      <Loader2 className="h-5 w-5 animate-spin" aria-hidden="true" />
      {text}
    </div>
  )
}

export function ErrorState({ message, onRetry }: { message: string; onRetry: () => void }) {
  return (
    <div className="flex flex-col items-start gap-3 rounded-lg border border-red-900 bg-red-950/40 p-4 text-red-300">
      <span className="flex items-center gap-2">
        <AlertCircle className="h-5 w-5" aria-hidden="true" />
        {message}
      </span>
      <button
        type="button"
        onClick={onRetry}
        className="flex items-center gap-2 rounded-md border border-red-800 px-3 py-1.5 text-sm hover:bg-red-900/40"
      >
        <RefreshCw className="h-4 w-4" aria-hidden="true" />
        Tentar novamente
      </button>
    </div>
  )
}

export function EmptyState({ icon: Icon, children }: { icon: typeof FolderKanban; children: ReactNode }) {
  return (
    <div className="flex flex-col items-center gap-3 rounded-lg border border-dashed border-zinc-800 p-10 text-center text-zinc-400">
      <Icon className="h-10 w-10 text-zinc-600" aria-hidden="true" />
      {children}
    </div>
  )
}

export function NoProjectState() {
  return (
    <EmptyState icon={FolderKanban}>
      <span>
        Nenhum projeto selecionado.{' '}
        <Link to="/projects" className="text-sky-400 hover:underline">
          Escolha ou crie um projeto
        </Link>
        .
      </span>
    </EmptyState>
  )
}

export function PrimaryButton({ onClick, icon, children }: { onClick: () => void; icon: ReactNode; children: ReactNode }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="flex items-center gap-2 rounded-md bg-sky-600 px-3 py-2 text-sm font-medium hover:bg-sky-500"
    >
      {icon}
      {children}
    </button>
  )
}
