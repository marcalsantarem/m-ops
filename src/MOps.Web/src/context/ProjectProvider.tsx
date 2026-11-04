import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { projectsApi, type ProjectDto } from '../api'
import { ProjectContext, type ProjectValue } from './projectContext'

const STORAGE_KEY = 'mops.projectId'

function readStoredId(): number | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    const id = raw ? Number(raw) : NaN
    return Number.isInteger(id) ? id : null
  } catch {
    return null
  }
}

function writeStoredId(id: number | null): void {
  try {
    if (id === null) localStorage.removeItem(STORAGE_KEY)
    else localStorage.setItem(STORAGE_KEY, String(id))
  } catch {
    // armazenamento indisponível: ignora
  }
}


export function ProjectProvider({ children }: { children: ReactNode }) {
  const [projects, setProjects] = useState<ProjectDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedId, setSelectedId] = useState<number | null>(() => readStoredId())

  const select = useCallback((id: number | null) => {
    setSelectedId(id)
    writeStoredId(id)
  }, [])

  const load = useCallback(async () => {
    try {
      const list = await projectsApi.list()
      setProjects(list)
      setError(null)
      setSelectedId((current) => {
        const next = list.some((p) => p.id === current) ? current : (list[0]?.id ?? null)
        writeStoredId(next)
        return next
      })
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar projetos')
    } finally {
      setLoading(false)
    }
  }, [])

  const reload = useCallback(async () => {
    setLoading(true)
    await load()
  }, [load])

  useEffect(() => {
    // carga inicial: loading já começa true; setState só ocorre após o await
    Promise.resolve().then(load)
  }, [load])

  const value = useMemo<ProjectValue>(
    () => ({
      projects,
      loading,
      error,
      selectedId,
      selected: projects.find((p) => p.id === selectedId) ?? null,
      select,
      reload,
    }),
    [projects, loading, error, selectedId, select, reload],
  )
  return <ProjectContext.Provider value={value}>{children}</ProjectContext.Provider>
}
