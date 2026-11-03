import { KanbanSquare } from 'lucide-react'

export default function HomePage() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-4 p-4">
      <KanbanSquare className="h-16 w-16 text-sky-400" aria-hidden="true" />
      <h1 className="text-4xl font-bold tracking-tight">M-Ops</h1>
    </main>
  )
}
