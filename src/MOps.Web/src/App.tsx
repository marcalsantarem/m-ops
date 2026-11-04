import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import Layout from './components/Layout'
import RequireAuth from './components/RequireAuth'
import { AuthProvider } from './context/AuthProvider'
import { ProjectProvider } from './context/ProjectProvider'
import LoginPage from './pages/LoginPage'
import PlaceholderPage from './pages/PlaceholderPage'
import ProjectsPage from './pages/ProjectsPage'
import BacklogPage from './pages/BacklogPage'
import SprintsPage from './pages/SprintsPage'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            element={
              <RequireAuth>
                <ProjectProvider>
                  <Layout />
                </ProjectProvider>
              </RequireAuth>
            }
          >
            <Route index element={<Navigate to="/projects" replace />} />
            <Route path="projects" element={<ProjectsPage />} />
            <Route path="backlog" element={<BacklogPage />} />
            <Route path="sprints" element={<SprintsPage />} />
            <Route path="kanban" element={<PlaceholderPage title="Kanban" />} />
            <Route path="*" element={<Navigate to="/projects" replace />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
