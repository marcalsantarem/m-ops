import { Bug, BookOpen, CheckSquare, ChevronDown, ChevronsUp, ChevronUp, Equal, type LucideIcon } from 'lucide-react'
import type { Priority, WorkItemStatus, WorkItemType } from '../api'

interface Meta {
  label: string
  icon: LucideIcon
  className: string
}

export const typeMeta: Record<WorkItemType, Meta> = {
  UserStory: { label: 'User story', icon: BookOpen, className: 'text-sky-400' },
  Task: { label: 'Task', icon: CheckSquare, className: 'text-amber-400' },
  Bug: { label: 'Bug', icon: Bug, className: 'text-red-400' },
}

export const priorityMeta: Record<Priority, Meta> = {
  Low: { label: 'Baixa', icon: ChevronDown, className: 'text-zinc-400' },
  Medium: { label: 'Média', icon: Equal, className: 'text-sky-400' },
  High: { label: 'Alta', icon: ChevronUp, className: 'text-orange-400' },
  Urgent: { label: 'Urgente', icon: ChevronsUp, className: 'text-red-400' },
}

export const statusMeta: Record<WorkItemStatus, { label: string; className: string }> = {
  ToDo: { label: 'A fazer', className: 'bg-zinc-700/50 text-zinc-300' },
  InProgress: { label: 'Em andamento', className: 'bg-sky-500/15 text-sky-300' },
  InReview: { label: 'Em revisão', className: 'bg-violet-500/15 text-violet-300' },
  Done: { label: 'Concluído', className: 'bg-emerald-500/15 text-emerald-300' },
}

export const workItemTypes = Object.keys(typeMeta) as WorkItemType[]
export const workItemStatuses = Object.keys(statusMeta) as WorkItemStatus[]
export const priorities = Object.keys(priorityMeta) as Priority[]
