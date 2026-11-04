import { useState } from 'react'
import { NavLink, Outlet } from 'react-router'
import { FolderKanban, KanbanSquare, ListTodo, LogOut, Menu, Timer, X } from 'lucide-react'
import { useAuth } from '../context/authContext'
import { useProjects } from '../context/projectContext'

const navItems = [
  { to: '/projects', label: 'Projetos', icon: FolderKanban, enabled: true },
  { to: '/backlog', label: 'Backlog', icon: ListTodo, enabled: true },
  { to: '/sprints', label: 'Sprints', icon: Timer, enabled: true },
  { to: '/kanban', label: 'Kanban', icon: KanbanSquare, enabled: false },
]

function ProjectSelector() {
  const { projects, selectedId, select, loading } = useProjects()
  return (
    <label className="block">
      <span className="mb-1 block text-xs font-medium uppercase tracking-wide text-zinc-500">Projeto</span>
      <select
        className="w-full rounded-md border border-zinc-700 bg-zinc-900 px-2 py-1.5 text-sm focus:border-sky-500 focus:outline-none"
        value={selectedId ?? ''}
        onChange={(e) => select(e.target.value ? Number(e.target.value) : null)}
        disabled={loading || projects.length === 0}
      >
        {projects.length === 0 && <option value="">{loading ? 'Carregando...' : 'Nenhum projeto'}</option>}
        {projects.map((p) => (
          <option key={p.id} value={p.id}>
            {p.name}
          </option>
        ))}
      </select>
    </label>
  )
}

function Sidebar({ onNavigate }: { onNavigate: () => void }) {
  const { logout } = useAuth()
  return (
    <nav className="flex h-full flex-col gap-6 p-4">
      <div className="flex items-center gap-2 text-lg font-bold">
        <KanbanSquare className="h-6 w-6 text-sky-400" aria-hidden="true" />
        M-Ops
      </div>
      <ProjectSelector />
      <ul className="flex flex-col gap-1">
        {navItems.map(({ to, label, icon: Icon, enabled }) => (
          <li key={to}>
            {enabled ? (
              <NavLink
                to={to}
                onClick={onNavigate}
                className={({ isActive }) =>
                  `flex items-center gap-2 rounded-md px-3 py-2 text-sm ${isActive ? 'bg-sky-500/15 text-sky-300' : 'text-zinc-300 hover:bg-zinc-800'}`
                }
              >
                <Icon className="h-4 w-4" aria-hidden="true" />
                {label}
              </NavLink>
            ) : (
              <span
                className="flex cursor-not-allowed items-center gap-2 rounded-md px-3 py-2 text-sm text-zinc-600"
                aria-disabled="true"
                title="Em breve"
              >
                <Icon className="h-4 w-4" aria-hidden="true" />
                {label}
              </span>
            )}
          </li>
        ))}
      </ul>
      <button
        type="button"
        onClick={logout}
        className="mt-auto flex items-center gap-2 rounded-md px-3 py-2 text-sm text-zinc-400 hover:bg-zinc-800 hover:text-zinc-100"
      >
        <LogOut className="h-4 w-4" aria-hidden="true" />
        Sair
      </button>
    </nav>
  )
}

export default function Layout() {
  const [open, setOpen] = useState(false)
  const close = () => setOpen(false)

  return (
    <div className="min-h-screen md:flex">
      <header className="flex items-center justify-between border-b border-zinc-800 px-4 py-3 md:hidden">
        <span className="flex items-center gap-2 font-bold">
          <KanbanSquare className="h-5 w-5 text-sky-400" aria-hidden="true" />
          M-Ops
        </span>
        <button type="button" onClick={() => setOpen(true)} className="rounded p-1 hover:bg-zinc-800" aria-label="Abrir menu">
          <Menu className="h-6 w-6" />
        </button>
      </header>

      {open && (
        <div className="fixed inset-0 z-40 md:hidden">
          <div className="absolute inset-0 bg-black/60" onClick={close} />
          <aside className="absolute inset-y-0 left-0 w-64 border-r border-zinc-800 bg-zinc-950">
            <button
              type="button"
              onClick={close}
              className="absolute right-3 top-3 rounded p-1 hover:bg-zinc-800"
              aria-label="Fechar menu"
            >
              <X className="h-5 w-5" />
            </button>
            <Sidebar onNavigate={close} />
          </aside>
        </div>
      )}

      <aside className="sticky top-0 hidden h-screen w-60 shrink-0 border-r border-zinc-800 md:block">
        <Sidebar onNavigate={close} />
      </aside>

      <main className="min-w-0 flex-1 p-4 md:p-8">
        <Outlet />
      </main>
    </div>
  )
}
