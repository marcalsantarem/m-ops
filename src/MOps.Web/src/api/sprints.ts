import { http } from './http'
import type { SprintDto, SprintInput } from './types'

const base = (projectId: number) => `/api/projects/${projectId}/sprints`

export const sprintsApi = {
  list: (projectId: number, signal?: AbortSignal) => http.get<SprintDto[]>(base(projectId), { signal }),
  get: (projectId: number, id: number, signal?: AbortSignal) =>
    http.get<SprintDto>(`${base(projectId)}/${id}`, { signal }),
  create: (projectId: number, input: SprintInput) => http.post<SprintDto>(base(projectId), input),
  update: (projectId: number, id: number, input: SprintInput) =>
    http.put<SprintDto>(`${base(projectId)}/${id}`, input),
  delete: (projectId: number, id: number) => http.delete(`${base(projectId)}/${id}`),
  activate: (projectId: number, id: number) => http.post<SprintDto>(`${base(projectId)}/${id}/activate`),
}
