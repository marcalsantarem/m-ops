import { createContext, useContext } from 'react'
import type { ProjectDto } from '../api'

export interface ProjectValue {
  projects: ProjectDto[]
  loading: boolean
  error: string | null
  selectedId: number | null
  selected: ProjectDto | null
  select: (id: number | null) => void
  reload: () => Promise<void>
}

export const ProjectContext = createContext<ProjectValue | null>(null)

export function useProjects(): ProjectValue {
  const ctx = useContext(ProjectContext)
  if (!ctx) throw new Error('useProjects fora do ProjectProvider')
  return ctx
}
