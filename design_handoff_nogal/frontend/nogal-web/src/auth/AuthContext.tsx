import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { login as loginRequest } from '../api/auth'
import { TOKEN_KEY } from '../api/client'
import type { UsuarioResumen } from '../types/auth'

const USER_KEY = 'nogal_usuario'

interface AuthContextValue {
  usuario: UsuarioResumen | null
  cargando: boolean
  login: (nombreUsuario: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioResumen | null>(null)
  const [cargando, setCargando] = useState(true)

  // Al montar, recupera la sesión guardada (si el token ya expiró, el
  // primer llamado a la API con auth:true fallará con 401 y RequireAuth
  // mandará de vuelta al login).
  useEffect(() => {
    const token = localStorage.getItem(TOKEN_KEY)
    const usuarioGuardado = localStorage.getItem(USER_KEY)

    if (token && usuarioGuardado) {
      try {
        setUsuario(JSON.parse(usuarioGuardado) as UsuarioResumen)
      } catch {
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem(USER_KEY)
      }
    }

    setCargando(false)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      usuario,
      cargando,
      async login(nombreUsuario, password) {
        const respuesta = await loginRequest(nombreUsuario, password)
        localStorage.setItem(TOKEN_KEY, respuesta.token)
        localStorage.setItem(USER_KEY, JSON.stringify(respuesta.usuario))
        setUsuario(respuesta.usuario)
      },
      logout() {
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem(USER_KEY)
        setUsuario(null)
      }
    }),
    [usuario, cargando]
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
