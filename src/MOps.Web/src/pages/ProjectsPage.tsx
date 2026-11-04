import { useState, type FormEvent } from 'react'
import { ExternalLink, FolderKanban, Pencil, Plus, Trash2 } from 'lucide-react'
import { projectsApi, type ProjectDto, type ProjectInput } from '../api'
import Modal from '../components/Modal'
import { useProjects } from '../context/projectContext'
import { ConfirmDelete, EmptyState, ErrorState, Field, FormActions, LoadingState, PrimaryButton } from '../components/ui'
import { borderFor, inputClass, splitSaveError, type FieldErrors } from '../components/formUtils'

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
      const { fields, form } = splitSaveError(err, ['name', 'description', 'gitrepositoryurl'])
      setErrors(fields)
      setFormError(form)
    } finally {
      setSaving(false)
    }
  }

  const border = (field: string) => borderFor(errors, field)

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
      <FormActions saving={saving} onCancel={onCancel} error={formError} />
    </form>
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
        <PrimaryButton onClick={() => setDialog({ kind: 'create' })} icon={<Plus className="h-4 w-4" aria-hidden="true" />}>
          Novo projeto
        </PrimaryButton>
      </div>

      {loading && projects.length === 0 ? (
        <LoadingState text="Carregando projetos..." />
      ) : error && projects.length === 0 ? (
        <ErrorState message={error} onRetry={() => void reload()} />
      ) : projects.length === 0 ? (
        <EmptyState icon={FolderKanban}>Nenhum projeto ainda. Crie o primeiro.</EmptyState>
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
          <ConfirmDelete
            message={
              <p>
                Excluir o projeto <strong>{dialog.project.name}</strong>? Todas as sprints, work items e commits dele serão
                excluídos. Esta ação não pode ser desfeita.
              </p>
            }
            onConfirm={() => projectsApi.delete(dialog.project.id)}
            onDone={afterDelete}
            onCancel={close}
          />
        </Modal>
      )}
    </div>
  )
}
