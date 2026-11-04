import { createContext, useContext } from 'react'

export interface AuthValue {
  token: string | null
  login: (token: string) => void
  logout: () => void
}

export const AuthContext = createContext<AuthValue | null>(null)

export function useAuth(): AuthValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth fora do AuthProvider')
  return ctx
}
