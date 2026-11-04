import { http } from './http'
import type { WorkItemDto, WorkItemInput, WorkItemQuery, WorkItemStatus } from './types'

export const workItemsApi = {
  list: (projectId: number, query: WorkItemQuery = {}, signal?: AbortSignal) =>
    http.get<WorkItemDto[]>(`/api/projects/${projectId}/work-items`, {
      signal,
      query: {
        sprintId: query.sprintId,
        onlyWithoutSprint: query.onlyWithoutSprint || undefined,
        status: query.status,
        type: query.type,
        keyword: query.keyword?.trim(),
      },
    }),
  get: (id: number, signal?: AbortSignal) => http.get<WorkItemDto>(`/api/work-items/${id}`, { signal }),
  create: (projectId: number, input: WorkItemInput) =>
    http.post<WorkItemDto>(`/api/projects/${projectId}/work-items`, input),
  update: (id: number, input: WorkItemInput) => http.put<WorkItemDto>(`/api/work-items/${id}`, input),
  delete: (id: number) => http.delete(`/api/work-items/${id}`),
  updateStatus: (id: number, status: WorkItemStatus) =>
    http.patch<WorkItemDto>(`/api/work-items/${id}/status`, { status }),
}
