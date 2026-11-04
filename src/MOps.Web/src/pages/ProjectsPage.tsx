import { useState, type FormEvent, type ReactNode } from 'react'
import { AlertCircle, ExternalLink, FolderKanban, Loader2, Pencil, Plus, RefreshCw, Trash2 } from 'lucide-react'
import { ApiError, projectsApi, type ProjectDto, type ProjectInput } from '../api'
import Modal from '../components/Modal'
import { useProjects } from '../context/projectContext'

type FieldErrors = Record<string, string[]>

/** Normaliza as chaves de ApiError.errors (ex.: "Name", "$.name") para minúsculas. */
function normalizeErrors(errors?: Record<string, string[]>): FieldErrors {
  const out: FieldErrors = {}
  for (const [key, messages] of Object.entries(errors ?? {})) {
    const k = key.replace(/^\$\./, '').toLowerCase()
    out[k] = [...(out[k] ?? []), ...messages]
  }
  return out
}

const inputClass =
  'w-full rounded-md border bg-zinc-950 px-3 py-2 text-sm focus:outline-none focus:border-sky-500'

function Field({
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

function ProjectForm({
  project,
  onSaved,
  onCancel,
}: {
  project: ProjectDto | null
  onSaved: (saved: ProjectDto) => void
  onCancel: () => void
}) {
  const [name, setName] = useState(project?.name ?? '')
  const [description, setDescription] = useState(project?.description ?? '')
  const [repoUrl, setRepoUrl] = useState(project?.gitRepositoryUrl ?? '')
  const [errors, setErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!name.trim()) {
      setErrors({ name: ['O nome é obrigatório.'] })
      return
    }
    const input: ProjectInput = {
      name: name.trim(),
      description: description.trim() || null,
      gitRepositoryUrl: repoUrl.trim() || null,
    }
    setSaving(true)
    setErrors({})
    setFormError(null)
    try {
      const saved = project ? await projectsApi.update(project.id, input) : await projectsApi.create(input)
      onSaved(saved)
    } catch (err) {
      if (err instanceof ApiError && err.errors) {
        const fe = normalizeErrors(err.errors)
        setErrors(fe)
        const known = ['name', 'description', 'gitrepositoryurl']
        const other = Object.entries(fe)
          .filter(([k]) => !known.includes(k))
          .flatMap(([, v]) => v)
        setFormError(other.length ? other.join(' ') : null)
      } else {
        setFormError(err instanceof Error ? err.message : 'Erro ao salvar.')
      }
    } finally {
      setSaving(false)
    }
  }

  const border = (field: string) => (errors[field] ? 'border-red-500' : 'border-zinc-700')

  return (
    <form onSubmit={handleSubmit} className="space-y-4" noValidate>
      <Field label="Nome *" name="name" errors={errors}>
        <input
          className={`${inputClass} ${border('name')}`}
          value={name}
          onChange={(e) => setName(e.target.value)}
          autoFocus
          maxLength={200}
        />
      </Field>
      <Field label="Descrição" name="description" errors={errors}>
        <textarea
          className={`${inputClass} ${border('description')} min-h-24`}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
      </Field>
      <Field label="URL do repositório Git" name="gitRepositoryUrl" errors={errors}>
        <input
          className={`${inputClass} ${border('gitrepositoryurl')}`}
          value={repoUrl}
          onChange={(e) => setRepoUrl(e.target.value)}
          placeholder="https://github.com/usuario/repo"
        />
      </Field>
      {formError && (
        <p className="text-sm text-red-400" role="alert">
          {formError}
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
    </form>
  )
}

function DeleteConfirm({
  project,
  onDone,
  onCancel,
}: {
  project: ProjectDto
  onDone: () => void
  onCancel: () => void
}) {
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function confirm() {
    setBusy(true)
    setError(null)
    try {
      await projectsApi.delete(project.id)
      onDone()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao excluir.')
      setBusy(false)
    }
  }

  return (
    <div className="space-y-4">
      <p className="text-sm text-zinc-300">
        Excluir o projeto <strong>{project.name}</strong>? Todas as sprints, work items e commits dele serão excluídos. Esta ação
        não pode ser desfeita.
      </p>
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

type Dialog = { kind: 'create' } | { kind: 'edit'; project: ProjectDto } | { kind: 'delete'; project: ProjectDto } | null

export default function ProjectsPage() {
  const { projects, loading, error, reload, select, selectedId } = useProjects()
  const [dialog, setDialog] = useState<Dialog>(null)
  const close = () => setDialog(null)

  async function afterSave(saved: ProjectDto) {
    close()
    await reload()
    if (selectedId === null) select(saved.id)
  }

  async function afterDelete() {
    close()
    await reload()
  }

  return (
    <div className="mx-auto max-w-5xl">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold">Projetos</h1>
        <button
          type="button"
          onClick={() => setDialog({ kind: 'create' })}
          className="flex items-center gap-2 rounded-md bg-sky-600 px-3 py-2 text-sm font-medium hover:bg-sky-500"
        >
          <Plus className="h-4 w-4" aria-hidden="true" />
          Novo projeto
        </button>
      </div>

      {loading && projects.length === 0 ? (
        <div className="flex items-center gap-2 text-zinc-400">
          <Loader2 className="h-5 w-5 animate-spin" aria-hidden="true" />
          Carregando projetos...
        </div>
      ) : error && projects.length === 0 ? (
        <div className="flex flex-col items-start gap-3 rounded-lg border border-red-900 bg-red-950/40 p-4 text-red-300">
          <span className="flex items-center gap-2">
            <AlertCircle className="h-5 w-5" aria-hidden="true" />
            {error}
          </span>
          <button
            type="button"
            onClick={() => void reload()}
            className="flex items-center gap-2 rounded-md border border-red-800 px-3 py-1.5 text-sm hover:bg-red-900/40"
          >
            <RefreshCw className="h-4 w-4" aria-hidden="true" />
            Tentar novamente
          </button>
        </div>
      ) : projects.length === 0 ? (
        <div className="flex flex-col items-center gap-3 rounded-lg border border-dashed border-zinc-800 p-10 text-center text-zinc-400">
          <FolderKanban className="h-10 w-10 text-zinc-600" aria-hidden="true" />
          Nenhum projeto ainda. Crie o primeiro.
        </div>
      ) : (
        <ul className="grid gap-3 sm:grid-cols-2">
          {projects.map((p) => (
            <li
              key={p.id}
              className={`flex flex-col gap-2 rounded-lg border bg-zinc-900 p-4 ${p.id === selectedId ? 'border-sky-700' : 'border-zinc-800'}`}
            >
              <div className="flex items-start justify-between gap-2">
                <button
                  type="button"
                  onClick={() => select(p.id)}
                  className="text-left font-semibold hover:text-sky-300"
                  title="Selecionar projeto"
                >
                  {p.name}
                </button>
                <div className="flex shrink-0 gap-1">
                  <button
                    type="button"
                    onClick={() => setDialog({ kind: 'edit', project: p })}
                    className="rounded p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-100"
                    aria-label={`Editar ${p.name}`}
                  >
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button
                    type="button"
                    onClick={() => setDialog({ kind: 'delete', project: p })}
                    className="rounded p-1.5 text-zinc-400 hover:bg-red-950 hover:text-red-400"
                    aria-label={`Excluir ${p.name}`}
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
              {p.description && <p className="whitespace-pre-line text-sm text-zinc-400">{p.description}</p>}
              {p.gitRepositoryUrl && (
                <a
                  href={p.gitRepositoryUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="flex items-center gap-1 break-all text-xs text-sky-400 hover:underline"
                >
                  <ExternalLink className="h-3 w-3 shrink-0" aria-hidden="true" />
                  {p.gitRepositoryUrl}
                </a>
              )}
            </li>
          ))}
        </ul>
      )}

      {(dialog?.kind === 'create' || dialog?.kind === 'edit') && (
        <Modal title={dialog.kind === 'create' ? 'Novo projeto' : 'Editar projeto'} onClose={close}>
          <ProjectForm project={dialog.kind === 'edit' ? dialog.project : null} onSaved={afterSave} onCancel={close} />
        </Modal>
      )}
      {dialog?.kind === 'delete' && (
        <Modal title="Excluir projeto" onClose={close}>
          <DeleteConfirm project={dialog.project} onDone={afterDelete} onCancel={close} />
        </Modal>
      )}
    </div>
  )
}
