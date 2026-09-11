import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate, type Location } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext'
import { ApiError } from '../../api/client'

export function LoginPage() {
  const { usuario, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [nombreUsuario, setNombreUsuario] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  // Ya hay sesión: no tiene sentido mostrar el login.
  if (usuario) {
    const destino = (location.state as { from?: Location })?.from?.pathname ?? '/admin'
    return <Navigate to={destino} replace />
  }

  async function entrar(evento: FormEvent) {
    evento.preventDefault()
    setError('')
    setEnviando(true)

    try {
      await login(nombreUsuario, password)
      navigate('/admin', { replace: true })
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        setError('Usuario o contraseña incorrectos.')
      } else {
        setError('No pudimos conectar con el servidor. Intenta de nuevo en un momento.')
      }
    } finally {
      setEnviando(false)
    }
  }

  return (
    <main
      style={{
        display: 'grid',
        placeItems: 'center',
        padding: 'calc(var(--space-8)*2) max(var(--space-4),4vw)',
        minHeight: '60vh'
      }}
    >
      <form
        onSubmit={entrar}
        style={{
          width: 'min(380px,100%)',
          border: '1px solid var(--color-divider)',
          borderRadius: 'var(--radius-md)',
          padding: 'var(--space-6)',
          display: 'flex',
          flexDirection: 'column',
          gap: 'var(--space-4)'
        }}
      >
        <div>
          <p
            style={{
              fontSize: 11,
              letterSpacing: '0.14em',
              textTransform: 'uppercase',
              color: 'var(--color-accent-700)',
              margin: 0
            }}
          >
            Acceso interno
          </p>
          <h2 style={{ fontWeight: 400, margin: 'var(--space-2) 0 0' }}>Panel de la fábrica</h2>
          <p className="text-muted" style={{ fontSize: 13, margin: 'var(--space-1) 0 0' }}>
            Solo para el equipo de Nogal.
          </p>
        </div>

        <div className="field">
          <label htmlFor="u">Usuario</label>
          <input
            id="u"
            className="input"
            value={nombreUsuario}
            onChange={(e) => setNombreUsuario(e.target.value)}
            placeholder="admin"
            autoComplete="username"
          />
        </div>

        <div className="field">
          <label htmlFor="c">Contraseña</label>
          <input
            id="c"
            className="input"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            autoComplete="current-password"
          />
        </div>

        {error && (
          <p
            role="alert"
            style={{
              fontSize: 12,
              color: 'var(--color-accent-700)',
              borderLeft: '1px solid var(--color-accent)',
              paddingLeft: 'var(--space-2)',
              margin: 0
            }}
          >
            {error}
          </p>
        )}

        <button className="btn btn-primary btn-block" type="submit" disabled={enviando}>
          {enviando ? 'Entrando…' : 'Entrar'}
        </button>

        {import.meta.env.DEV && (
          <p className="text-muted" style={{ fontSize: 11, margin: 0, textAlign: 'center' }}>
            Demo (solo en desarrollo): admin / nogal2026
          </p>
        )}
      </form>
    </main>
  )
}
