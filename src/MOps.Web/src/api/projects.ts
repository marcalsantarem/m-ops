import { http } from './http'
import type { ProjectDto, ProjectInput } from './types'

export const projectsApi = {
  list: (signal?: AbortSignal) => http.get<ProjectDto[]>('/api/projects', { signal }),
  get: (id: number, signal?: AbortSignal) => http.get<ProjectDto>(`/api/projects/${id}`, { signal }),
  create: (input: ProjectInput) => http.post<ProjectDto>('/api/projects', input),
  update: (id: number, input: ProjectInput) => http.put<ProjectDto>(`/api/projects/${id}`, input),
  delete: (id: number) => http.delete(`/api/projects/${id}`),
}
