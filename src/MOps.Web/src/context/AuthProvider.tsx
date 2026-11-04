import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useNavigate } from 'react-router'
import { clearToken, getToken, onUnauthorized, setToken } from '../api'
import { AuthContext } from './authContext'


export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setTokenState] = useState<string | null>(() => getToken())
  const navigate = useNavigate()

  const login = useCallback((value: string) => {
    setToken(value)
    setTokenState(value)
  }, [])

  const logout = useCallback(() => {
    clearToken()
    setTokenState(null)
    navigate('/login', { replace: true })
  }, [navigate])

  useEffect(() => onUnauthorized(logout), [logout])

  const value = useMemo(() => ({ token, login, logout }), [token, login, logout])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
