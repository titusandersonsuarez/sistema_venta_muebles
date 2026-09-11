import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import * as authApi from '../api/auth'
import { ApiError } from '../api/client'
import type { UsuarioResumen } from '../types/auth'

interface AuthContextValue {
  usuario: UsuarioResumen | null
  cargando: boolean
  login: (nombreUsuario: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

/**
 * Fuente de verdad de sesión: la cookie HttpOnly `nogal_auth` que
 * gestiona el backend. En el frontend solo cacheamos los datos del
 * usuario en estado — no hay tokens en localStorage.
 *
 * Al montar consultamos /api/auth/me para reconciliar: si el navegador
 * tiene la cookie válida, obtenemos el usuario; si no (401), quedamos
 * anónimos y RequireAuth mandará al login.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioResumen | null>(null)
  const [cargando, setCargando] = useState(true)

  useEffect(() => {
    let cancelado = false
    ;(async () => {
      try {
        const me = await authApi.me()
        if (!cancelado) setUsuario(me)
      } catch (error) {
        if (!cancelado && error instanceof ApiError && error.status === 401) {
          setUsuario(null)
        }
        // Otros errores (red caída) los tratamos como anónimo también;
        // el usuario podrá reintentar iniciando sesión.
      } finally {
        if (!cancelado) setCargando(false)
      }
    })()
    return () => {
      cancelado = true
    }
  }, [])

  const login = useCallback(async (nombreUsuario: string, password: string) => {
    const respuesta = await authApi.login(nombreUsuario, password)
    setUsuario(respuesta.usuario)
  }, [])

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } finally {
      setUsuario(null)
    }
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({ usuario, cargando, login, logout }),
    [usuario, cargando, login, logout]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth debe usarse dentro de <AuthProvider>')
  }
  return context
}
