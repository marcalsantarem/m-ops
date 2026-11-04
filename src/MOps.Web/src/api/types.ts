// Espelha src/MOps.Core/Dtos (JSON camelCase, enums como string, datas ISO 8601).

export type WorkItemType = 'UserStory' | 'Task' | 'Bug'
export type WorkItemStatus = 'ToDo' | 'InProgress' | 'InReview' | 'Done'
export type Priority = 'Low' | 'Medium' | 'High' | 'Urgent'

export interface ProjectDto {
  id: number
  name: string
  description: string | null
  gitRepositoryUrl: string | null
  createdAt: string
}

export interface ProjectInput {
  name: string
  description?: string | null
  gitRepositoryUrl?: string | null
}

export interface SprintDto {
  id: number
  projectId: number
  name: string
  startDate: string
  endDate: string
  isActive: boolean
}

export interface SprintInput {
  name: string
  startDate: string
  endDate: string
}

export interface WorkItemDto {
  id: number
  projectId: number
  sprintId: number | null
  title: string
  description: string | null
  type: WorkItemType
  status: WorkItemStatus
  priority: Priority
  createdAt: string
  updatedAt: string
}

export interface WorkItemInput {
  title: string
  description?: string | null
  type?: WorkItemType | null
  status?: WorkItemStatus | null
  priority?: Priority | null
  sprintId?: number | null
}

export interface WorkItemQuery {
  sprintId?: number
  onlyWithoutSprint?: boolean
  status?: WorkItemStatus
  type?: WorkItemType
  keyword?: string
}

export interface CommitRecordDto {
  id: number
  commitHash: string
  message: string
  author: string
  committedAt: string
}

export interface CommitHookResult {
  commitHash: string
  linked: number[]
  alreadyLinked: number[]
  notFound: number[]
  noReferences: boolean
  hasAnyLink: boolean
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  errors?: Record<string, string[]>
}
