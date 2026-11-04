import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { CalendarDays, CheckCircle2, Loader2, Pencil, Plus, Timer, Trash2 } from 'lucide-react'
import { sprintsApi, type SprintDto, type SprintInput } from '../api'
import Modal from '../components/Modal'
import { ConfirmDelete, EmptyState, ErrorState, Field, FormActions, LoadingState, NoProjectState, PrimaryButton } from '../components/ui'
import { borderFor, inputClass, splitSaveError, type FieldErrors } from '../components/formUtils'
import { useProjects } from '../context/projectContext'

const dateFmt = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', timeZone: 'UTC' })

/** As datas são gravadas como meia-noite UTC; exibe/edita em UTC para não deslocar o dia. */
const formatDate = (iso: string) => dateFmt.format(new Date(iso))
const toInputDate = (iso: string) => new Date(iso).toISOString().slice(0, 10)
const fromInputDate = (d: string) => new Date(`${d}T00:00:00Z`).toISOString()

function addDays(days: number): string {
  const d = new Date()
  d.setDate(d.getDate() + days)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function SprintForm({
  projectId,
  sprint,
  onSaved,
  onCancel,
}: {
  projectId: number
  sprint: SprintDto | null
  onSaved: () => void
  onCancel: () => void
}) {
  const [name, setName] = useState(sprint?.name ?? '')
  const [startDate, setStartDate] = useState(sprint ? toInputDate(sprint.startDate) : addDays(0))
  const [endDate, setEndDate] = useState(sprint ? toInputDate(sprint.endDate) : addDays(14))
  const [errors, setErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const fe: FieldErrors = {}
    if (!name.trim()) fe.name = ['O nome é obrigatório.']
    if (!startDate) fe.startdate = ['Informe a data de início.']
    if (!endDate) fe.enddate = ['Informe a data de término.']
    else if (startDate && endDate < startDate) fe.enddate = ['O término deve ser igual ou posterior ao início.']
    if (Object.keys(fe).length) {
      setErrors(fe)
      return
    }
    const input: SprintInput = { name: name.trim(), startDate: fromInputDate(startDate), endDate: fromInputDate(endDate) }
    setSaving(true)
    setErrors({})
    setFormError(null)
    try {
      if (sprint) await sprintsApi.update(projectId, sprint.id, input)
      else await sprintsApi.create(projectId, input)
      onSaved()
    } catch (err) {
      const { fields, form } = splitSaveError(err, ['name', 'startdate', 'enddate'])
      setErrors(fields)
      setFormError(form)
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4" noValidate>
      <Field label="Nome *" name="name" errors={errors}>
        <input
          className={`${inputClass} ${borderFor(errors, 'name')}`}
          value={name}
          onChange={(e) => setName(e.target.value)}
          autoFocus
          maxLength={200}
        />
      </Field>
      <div className="grid gap-4 sm:grid-cols-2">
        <Field label="Início *" name="startDate" errors={errors}>
          <input
            type="date"
            className={`${inputClass} ${borderFor(errors, 'startdate')} [color-scheme:dark]`}
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
          />
        </Field>
        <Field label="Término *" name="endDate" errors={errors}>
          <input
            type="date"
            className={`${inputClass} ${borderFor(errors, 'enddate')} [color-scheme:dark]`}
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
          />
        </Field>
      </div>
      <FormActions saving={saving} onCancel={onCancel} error={formError} />
    </form>
  )
}

type Dialog = { kind: 'create' } | { kind: 'edit'; sprint: SprintDto } | { kind: 'delete'; sprint: SprintDto } | null

export default function SprintsPage() {
  const { selected } = useProjects()
  if (!selected) return <NoProjectState />
  return <ProjectSprints key={selected.id} projectId={selected.id} projectName={selected.name} />
}

function ProjectSprints({ projectId, projectName }: { projectId: number; projectName: string }) {
  const [sprints, setSprints] = useState<SprintDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [activatingId, setActivatingId] = useState<number | null>(null)
  const [dialog, setDialog] = useState<Dialog>(null)
  const close = () => setDialog(null)

  const load = useCallback(
    async (signal?: AbortSignal) => {
      setLoading(true)
      setError(null)
      try {
        const data = await sprintsApi.list(projectId, signal)
        setSprints(data)
      } catch (err) {
        if (signal?.aborted) return
        setError(err instanceof Error ? err.message : 'Erro ao carregar sprints.')
      } finally {
        if (!signal?.aborted) setLoading(false)
      }
    },
    [projectId],
  )

  useEffect(() => {
    const ctrl = new AbortController()
    // setState só após o microtask (evita setState síncrono no effect)
    void Promise.resolve().then(() => load(ctrl.signal))
    return () => ctrl.abort()
  }, [load])

  async function activate(s: SprintDto) {
    setActivatingId(s.id)
    setActionError(null)
    try {
      await sprintsApi.activate(projectId, s.id)
      await load()
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Erro ao ativar a sprint.')
    } finally {
      setActivatingId(null)
    }
  }

  async function afterChange() {
    close()
    await load()
  }

  return (
    <div className="mx-auto max-w-5xl">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">Sprints</h1>
          <p className="text-sm text-zinc-500">{projectName}</p>
        </div>
        <PrimaryButton onClick={() => setDialog({ kind: 'create' })} icon={<Plus className="h-4 w-4" aria-hidden="true" />}>
          Nova sprint
        </PrimaryButton>
      </div>

      {actionError && (
        <p className="mb-4 rounded-md border border-red-900 bg-red-950/40 px-3 py-2 text-sm text-red-300" role="alert">
          {actionError}
        </p>
      )}

      {loading && sprints.length === 0 ? (
        <LoadingState text="Carregando sprints..." />
      ) : error && sprints.length === 0 ? (
        <ErrorState message={error} onRetry={() => void load()} />
      ) : sprints.length === 0 ? (
        <EmptyState icon={Timer}>Nenhuma sprint neste projeto. Crie a primeira.</EmptyState>
      ) : (
        <ul className="space-y-2">
          {sprints.map((s) => (
            <li
              key={s.id}
              className={`flex flex-wrap items-center justify-between gap-3 rounded-lg border bg-zinc-900 p-4 ${s.isActive ? 'border-emerald-800' : 'border-zinc-800'}`}
            >
              <div className="min-w-0">
                <div className="flex flex-wrap items-center gap-2">
                  <span className="font-semibold">{s.name}</span>
                  {s.isActive && (
                    <span className="rounded-full bg-emerald-500/15 px-2 py-0.5 text-xs font-medium text-emerald-300">Ativa</span>
                  )}
                </div>
                <span className="mt-1 flex items-center gap-1 text-sm text-zinc-400">
                  <CalendarDays className="h-4 w-4" aria-hidden="true" />
                  {formatDate(s.startDate)} – {formatDate(s.endDate)}
                </span>
              </div>
              <div className="flex shrink-0 items-center gap-1">
                {!s.isActive && (
                  <button
                    type="button"
                    onClick={() => void activate(s)}
                    disabled={activatingId !== null}
                    className="flex items-center gap-1 rounded-md border border-zinc-700 px-2 py-1 text-sm text-zinc-300 hover:border-emerald-700 hover:text-emerald-300 disabled:opacity-60"
                  >
                    {activatingId === s.id ? (
                      <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
                    ) : (
                      <CheckCircle2 className="h-4 w-4" aria-hidden="true" />
                    )}
                    Ativar
                  </button>
                )}
                <button
                  type="button"
                  onClick={() => setDialog({ kind: 'edit', sprint: s })}
                  className="rounded p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-100"
                  aria-label={`Editar ${s.name}`}
                >
                  <Pencil className="h-4 w-4" />
                </button>
                <button
                  type="button"
                  onClick={() => setDialog({ kind: 'delete', sprint: s })}
                  className="rounded p-1.5 text-zinc-400 hover:bg-red-950 hover:text-red-400"
                  aria-label={`Excluir ${s.name}`}
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}

      {(dialog?.kind === 'create' || dialog?.kind === 'edit') && (
        <Modal title={dialog.kind === 'create' ? 'Nova sprint' : 'Editar sprint'} onClose={close}>
          <SprintForm
            projectId={projectId}
            sprint={dialog.kind === 'edit' ? dialog.sprint : null}
            onSaved={afterChange}
            onCancel={close}
          />
        </Modal>
      )}
      {dialog?.kind === 'delete' && (
        <Modal title="Excluir sprint" onClose={close}>
          <ConfirmDelete
            message={
              <p>
                Excluir a sprint <strong>{dialog.sprint.name}</strong>? Os work items dela não serão excluídos: voltam para o
                backlog (sem sprint). Esta ação não pode ser desfeita.
              </p>
            }
            onConfirm={() => sprintsApi.delete(projectId, dialog.sprint.id)}
            onDone={afterChange}
            onCancel={close}
          />
        </Modal>
      )}
    </div>
  )
}
