import { http } from './http'
import type { CommitRecordDto } from './types'

export const commitsApi = {
  listByWorkItem: (workItemId: number, signal?: AbortSignal) =>
    http.get<CommitRecordDto[]>(`/api/work-items/${workItemId}/commits`, { signal }),
}
