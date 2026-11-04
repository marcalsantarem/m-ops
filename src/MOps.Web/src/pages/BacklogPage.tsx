import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { ListTodo, Pencil, Plus, Search, Trash2 } from 'lucide-react'
import {
  sprintsApi,
  workItemsApi,
  type Priority,
  type SprintDto,
  type WorkItemDto,
  type WorkItemInput,
  type WorkItemQuery,
  type WorkItemStatus,
  type WorkItemType,
} from '../api'
import Modal from '../components/Modal'
import {
  ConfirmDelete,
  EmptyState,
  ErrorState,
  Field,
  FormActions,
  LoadingState,
  NoProjectState,
  PrimaryButton,
} from '../components/ui'
import { borderFor, inputClass, selectClass, splitSaveError, type FieldErrors } from '../components/formUtils'
import { priorities, priorityMeta, statusMeta, typeMeta, workItemStatuses, workItemTypes } from '../components/workItemMeta'
import { useProjects } from '../context/projectContext'

const toSprintId = (v: string) => (v ? Number(v) : null)

function SprintOptions({ sprints }: { sprints: SprintDto[] }) {
  return (
    <>
      <option value="">Sem sprint</option>
      {sprints.map((s) => (
        <option key={s.id} value={s.id}>
          {s.name}
          {s.isActive ? ' (ativa)' : ''}
        </option>
      ))}
    </>
  )
}

/** Monta o corpo do PUT a partir de um item existente. */
const toInput = (w: WorkItemDto): WorkItemInput => ({
  title: w.title,
  description: w.description,
  type: w.type,
  status: w.status,
  priority: w.priority,
  sprintId: w.sprintId,
})

function WorkItemForm({
  projectId,
  item,
  sprints,
  onSaved,
  onCancel,
}: {
  projectId: number
  item: WorkItemDto | null
  sprints: SprintDto[]
  onSaved: () => void
  onCancel: () => void
}) {
  const [title, setTitle] = useState(item?.title ?? '')
  const [description, setDescription] = useState(item?.description ?? '')
  const [type, setType] = useState<WorkItemType>(item?.type ?? 'Task')
  const [status, setStatus] = useState<WorkItemStatus>(item?.status ?? 'ToDo')
  const [priority, setPriority] = useState<Priority>(item?.priority ?? 'Medium')
  const [sprintId, setSprintId] = useState<number | null>(item?.sprintId ?? null)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!title.trim()) {
      setErrors({ title: ['O título é obrigatório.'] })
      return
    }
    const input: WorkItemInput = {
      title: title.trim(),
      description: description.trim() ? description : null,
      type,
      status,
      priority,
      sprintId,
    }
    setSaving(true)
    setErrors({})
    setFormError(null)
    try {
      if (item) await workItemsApi.update(item.id, input)
      else await workItemsApi.create(projectId, input)
      onSaved()
    } catch (err) {
      const { fields, form } = splitSaveError(err, ['title', 'description', 'type', 'status', 'priority', 'sprintid'])
      setErrors(fields)
      setFormError(form)
    } finally {
      setSaving(false)
    }
  }

  const sel = (field: string) => `${inputClass} ${borderFor(errors, field)}`

  return (
    <form onSubmit={handleSubmit} className="space-y-4" noValidate>
      <Field label="Título *" name="title" errors={errors}>
        <input className={sel('title')} value={title} onChange={(e) => setTitle(e.target.value)} autoFocus maxLength={300} />
      </Field>
      <Field label="Descrição (Markdown)" name="description" errors={errors}>
        <textarea
          className={`${sel('description')} min-h-32 font-mono`}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
      </Field>
      <div className="grid gap-4 sm:grid-cols-2">
        <Field label="Tipo" name="type" errors={errors}>
          <select className={sel('type')} value={type} onChange={(e) => setType(e.target.value as WorkItemType)}>
            {workItemTypes.map((t) => (
              <option key={t} value={t}>
                {typeMeta[t].label}
              </option>
            ))}
          </select>
        </Field>
        <Field label="Status" name="status" errors={errors}>
          <select className={sel('status')} value={status} onChange={(e) => setStatus(e.target.value as WorkItemStatus)}>
            {workItemStatuses.map((s) => (
              <option key={s} value={s}>
                {statusMeta[s].label}
              </option>
            ))}
          </select>
        </Field>
        <Field label="Prioridade" name="priority" errors={errors}>
          <select className={sel('priority')} value={priority} onChange={(e) => setPriority(e.target.value as Priority)}>
            {priorities.map((p) => (
              <option key={p} value={p}>
                {priorityMeta[p].label}
              </option>
            ))}
          </select>
        </Field>
        <Field label="Sprint" name="sprintId" errors={errors}>
          <select className={sel('sprintid')} value={sprintId ?? ''} onChange={(e) => setSprintId(toSprintId(e.target.value))}>
            <SprintOptions sprints={sprints} />
          </select>
        </Field>
      </div>
      <FormActions saving={saving} onCancel={onCancel} error={formError} />
    </form>
  )
}

function useDebounced<T>(value: T, ms: number): T {
  const [v, setV] = useState(value)
  useEffect(() => {
    const t = setTimeout(() => setV(value), ms)
    return () => clearTimeout(t)
  }, [value, ms])
  return v
}

type Dialog = { kind: 'create' } | { kind: 'edit'; item: WorkItemDto } | { kind: 'delete'; item: WorkItemDto } | null

/** '' = todas, 'none' = sem sprint, ou o id da sprint. */
type SprintFilter = string

export default function BacklogPage() {
  const { selected } = useProjects()
  if (!selected) return <NoProjectState />
  return <ProjectBacklog key={selected.id} projectId={selected.id} projectName={selected.name} />
}

function ProjectBacklog({ projectId, projectName }: { projectId: number; projectName: string }) {
  const [sprints, setSprints] = useState<SprintDto[]>([])
  const [items, setItems] = useState<WorkItemDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [movingId, setMovingId] = useState<number | null>(null)
  const [dialog, setDialog] = useState<Dialog>(null)
  const close = () => setDialog(null)

  const [sprintFilter, setSprintFilter] = useState<SprintFilter>('')
  const [statusFilter, setStatusFilter] = useState<WorkItemStatus | ''>('')
  const [typeFilter, setTypeFilter] = useState<WorkItemType | ''>('')
  const [keyword, setKeyword] = useState('')
  const debouncedKeyword = useDebounced(keyword, 300)

  useEffect(() => {
    const ctrl = new AbortController()
    sprintsApi.list(projectId, ctrl.signal).then(setSprints, () => {
      /* sem sprints: os selects mostram só "Sem sprint" */
    })
    return () => ctrl.abort()
  }, [projectId])

  const query = useMemo<WorkItemQuery>(
    () => ({
      sprintId: sprintFilter && sprintFilter !== 'none' ? Number(sprintFilter) : undefined,
      onlyWithoutSprint: sprintFilter === 'none',
      status: statusFilter || undefined,
      type: typeFilter || undefined,
      keyword: debouncedKeyword,
    }),
    [sprintFilter, statusFilter, typeFilter, debouncedKeyword],
  )

  const load = useCallback(
    async (signal?: AbortSignal) => {
      setLoading(true)
      setError(null)
      try {
        setItems(await workItemsApi.list(projectId, query, signal))
      } catch (err) {
        if (signal?.aborted) return
        setError(err instanceof Error ? err.message : 'Erro ao carregar o backlog.')
      } finally {
        if (!signal?.aborted) setLoading(false)
      }
    },
    [projectId, query],
  )

  useEffect(() => {
    const ctrl = new AbortController()
    // setState só após o microtask (evita setState síncrono no effect)
    void Promise.resolve().then(() => load(ctrl.signal))
    return () => ctrl.abort()
  }, [load])

  const sprintName = useMemo(() => new Map(sprints.map((s) => [s.id, s.name])), [sprints])
  const hasFilters = !!(sprintFilter || statusFilter || typeFilter || debouncedKeyword.trim())

  async function moveToSprint(item: WorkItemDto, sprintId: number | null) {
    if (sprintId === item.sprintId) return
    setMovingId(item.id)
    setActionError(null)
    try {
      await workItemsApi.update(item.id, { ...toInput(item), sprintId })
      await load()
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Erro ao mover o item.')
    } finally {
      setMovingId(null)
    }
  }

  async function afterChange() {
    close()
    await load()
  }

  return (
    <div className="mx-auto max-w-6xl">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">Backlog</h1>
          <p className="text-sm text-zinc-500">{projectName}</p>
        </div>
        <PrimaryButton onClick={() => setDialog({ kind: 'create' })} icon={<Plus className="h-4 w-4" aria-hidden="true" />}>
          Novo item
        </PrimaryButton>
      </div>

      <div className="mb-4 grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
        <label className="relative block">
          <span className="sr-only">Buscar</span>
          <Search className="pointer-events-none absolute left-2.5 top-2 h-4 w-4 text-zinc-500" aria-hidden="true" />
          <input
            className={`${selectClass} w-full pl-8`}
            placeholder="Buscar por palavra-chave"
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
          />
        </label>
        <select
          className={selectClass}
          aria-label="Filtrar por sprint"
          value={sprintFilter}
          onChange={(e) => setSprintFilter(e.target.value)}
        >
          <option value="">Todas as sprints</option>
          <option value="none">Sem sprint</option>
          {sprints.map((s) => (
            <option key={s.id} value={s.id}>
              {s.name}
              {s.isActive ? ' (ativa)' : ''}
            </option>
          ))}
        </select>
        <select
          className={selectClass}
          aria-label="Filtrar por status"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as WorkItemStatus | '')}
        >
          <option value="">Todos os status</option>
          {workItemStatuses.map((s) => (
            <option key={s} value={s}>
              {statusMeta[s].label}
            </option>
          ))}
        </select>
        <select
          className={selectClass}
          aria-label="Filtrar por tipo"
          value={typeFilter}
          onChange={(e) => setTypeFilter(e.target.value as WorkItemType | '')}
        >
          <option value="">Todos os tipos</option>
          {workItemTypes.map((t) => (
            <option key={t} value={t}>
              {typeMeta[t].label}
            </option>
          ))}
        </select>
      </div>

      {actionError && (
        <p className="mb-4 rounded-md border border-red-900 bg-red-950/40 px-3 py-2 text-sm text-red-300" role="alert">
          {actionError}
        </p>
      )}

      {loading && items.length === 0 ? (
        <LoadingState text="Carregando backlog..." />
      ) : error && items.length === 0 ? (
        <ErrorState message={error} onRetry={() => void load()} />
      ) : items.length === 0 ? (
        <EmptyState icon={ListTodo}>
          {hasFilters ? 'Nenhum item corresponde aos filtros.' : 'Nenhum work item neste projeto. Crie o primeiro.'}
        </EmptyState>
      ) : (
        <ul className={`divide-y divide-zinc-800 rounded-lg border border-zinc-800 bg-zinc-900 ${loading ? 'opacity-60' : ''}`}>
          {items.map((w) => {
            const T = typeMeta[w.type]
            const P = priorityMeta[w.priority]
            return (
              <li key={w.id} className="flex flex-col gap-2 p-3 sm:flex-row sm:items-center sm:gap-3">
                <div className="flex min-w-0 flex-1 items-start gap-2">
                  <T.icon className={`mt-0.5 h-4 w-4 shrink-0 ${T.className}`} aria-label={T.label} />
                  <div className="min-w-0">
                    <button
                      type="button"
                      onClick={() => setDialog({ kind: 'edit', item: w })}
                      className="text-left font-medium hover:text-sky-300"
                    >
                      <span className="mr-1 text-zinc-500">#{w.id}</span>
                      {w.title}
                    </button>
                    <div className="mt-1 flex flex-wrap items-center gap-2 text-xs">
                      <span className={`rounded-full px-2 py-0.5 ${statusMeta[w.status].className}`}>
                        {statusMeta[w.status].label}
                      </span>
                      <span className={`flex items-center gap-0.5 ${P.className}`} title="Prioridade">
                        <P.icon className="h-3.5 w-3.5" aria-hidden="true" />
                        {P.label}
                      </span>
                      <span className="text-zinc-500">{T.label}</span>
                    </div>
                  </div>
                </div>
                <div className="flex shrink-0 items-center gap-1 pl-6 sm:pl-0">
                  <select
                    className={`${selectClass} max-w-48 text-xs`}
                    aria-label={`Sprint de #${w.id}`}
                    title={w.sprintId ? (sprintName.get(w.sprintId) ?? 'Sprint') : 'Sem sprint'}
                    value={w.sprintId ?? ''}
                    disabled={movingId !== null}
                    onChange={(e) => void moveToSprint(w, toSprintId(e.target.value))}
                  >
                    <SprintOptions sprints={sprints} />
                  </select>
                  <button
                    type="button"
                    onClick={() => setDialog({ kind: 'edit', item: w })}
                    className="rounded p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-100"
                    aria-label={`Editar #${w.id}`}
                  >
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button
                    type="button"
                    onClick={() => setDialog({ kind: 'delete', item: w })}
                    className="rounded p-1.5 text-zinc-400 hover:bg-red-950 hover:text-red-400"
                    aria-label={`Excluir #${w.id}`}
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </li>
            )
          })}
        </ul>
      )}

      {(dialog?.kind === 'create' || dialog?.kind === 'edit') && (
        <Modal title={dialog.kind === 'create' ? 'Novo work item' : `Editar #${dialog.item.id}`} onClose={close}>
          <WorkItemForm
            projectId={projectId}
            item={dialog.kind === 'edit' ? dialog.item : null}
            sprints={sprints}
            onSaved={afterChange}
            onCancel={close}
          />
        </Modal>
      )}
      {dialog?.kind === 'delete' && (
        <Modal title="Excluir work item" onClose={close}>
          <ConfirmDelete
            message={
              <p>
                Excluir <strong>#{dialog.item.id} {dialog.item.title}</strong>? Esta ação não pode ser desfeita.
              </p>
            }
            onConfirm={() => workItemsApi.delete(dialog.item.id)}
            onDone={afterChange}
            onCancel={close}
          />
        </Modal>
      )}
    </div>
  )
}
