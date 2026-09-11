import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

interface Modulo {
  path: string
  nombre: string
  titulo: string
  subtitulo: string
}

const MODULOS: Modulo[] = [
  {
    path: '/admin',
    nombre: 'Resumen',
    titulo: 'Resumen',
    subtitulo: 'Ventas, pedidos y producción de un vistazo.'
  },
  {
    path: '/admin/pedidos',
    nombre: 'Pedidos',
    titulo: 'Pedidos',
    subtitulo: 'Seguimiento de despachos, ciudades y estado de pago.'
  },
  {
    path: '/admin/productos',
    nombre: 'Productos',
    titulo: 'Productos',
    subtitulo: 'Catálogo, precios, imágenes y estado de cada mueble.'
  },
  {
    path: '/admin/produccion',
    nombre: 'Producción',
    titulo: 'Producción',
    subtitulo: 'Etapas del taller, capacidad e inventario de materiales.'
  }
]

export function AdminLayout() {
  const { usuario, logout } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const moduloActivo =
    MODULOS.find((m) => (m.path === '/admin' ? location.pathname === m.path : location.pathname.startsWith(m.path))) ??
    MODULOS[0]

  async function salir() {
    await logout()
    navigate('/admin/login', { replace: true })
  }

  const iniciales = usuario?.nombre
    ? usuario.nombre
        .split(' ')
        .filter(Boolean)
        .map((parte) => parte[0])
        .slice(0, 2)
        .join('')
        .toUpperCase()
    : '—'

  return (
    <main style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'flex-start', gap: 0 }}>
      <aside
        style={{
          flex: '1 1 210px',
          maxWidth: 236,
          alignSelf: 'stretch',
          borderRight: '1px solid var(--color-divider)',
          padding: 'var(--space-6) var(--space-4)',
          display: 'flex',
          flexDirection: 'column',
          gap: 'var(--space-2)',
          position: 'sticky',
          top: 0
        }}
      >
        <div style={{ marginBottom: 'var(--space-4)' }}>
          <div className="nav-brand" style={{ margin: 0 }}>
            Nogal
          </div>
          <p
            className="text-muted"
            style={{ fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', margin: 0 }}
          >
            Panel de fábrica
          </p>
        </div>

        <h6 style={{ marginBottom: 'var(--space-1)' }}>Módulos</h6>

        {MODULOS.map((modulo) => (
          <NavLink
            key={modulo.path}
            to={modulo.path}
            end={modulo.path === '/admin'}
            style={({ isActive }) => ({
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              gap: 'var(--space-2)',
              width: '100%',
              textAlign: 'left',
              cursor: 'pointer',
              fontFamily: 'var(--font-heading)',
              fontSize: 15,
              color: 'var(--color-text)',
              padding: 'var(--space-2) var(--space-3)',
              borderRadius: 'var(--radius-md)',
              border: `1px solid ${isActive ? 'var(--color-accent)' : 'transparent'}`,
              background: isActive ? 'var(--color-accent-100)' : 'transparent',
              textDecoration: 'none'
            })}
          >
            <span>{modulo.nombre}</span>
          </NavLink>
        ))}

        <hr className="hr" style={{ margin: 'var(--space-3) 0' }} />

        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)', alignItems: 'flex-start' }}>
          <button className="btn btn-ghost" style={{ fontSize: 13 }} onClick={() => navigate('/')}>
            Ver la tienda
          </button>
          <button className="btn btn-ghost" style={{ fontSize: 13 }} onClick={salir}>
            Cerrar sesión
          </button>
        </div>
      </aside>

      <div
        style={{
          flex: '100 1 340px',
          minWidth: 0,
          padding: 'var(--space-6) max(var(--space-4),3vw) calc(var(--space-8)*1.5)'
        }}
      >
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            gap: 'var(--space-4)',
            flexWrap: 'wrap',
            paddingBottom: 'var(--space-4)',
            borderBottom: '1px solid var(--color-divider)'
          }}
        >
          <div>
            <h1 style={{ fontWeight: 400, fontSize: 32, margin: 0 }}>{moduloActivo.titulo}</h1>
            <p className="text-muted" style={{ fontSize: 13, margin: 0 }}>
              {moduloActivo.subtitulo}
            </p>
          </div>

          <div style={{ display: 'flex', gap: 'var(--space-3)', alignItems: 'center', flexWrap: 'wrap' }}>
            <input className="input" placeholder="Buscar pedido, cliente o mueble…" style={{ width: 'min(280px,60vw)' }} />
            <button className="btn btn-secondary">Exportar CSV</button>
            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
              <span
                style={{
                  width: 34,
                  height: 34,
                  borderRadius: '50%',
                  border: '1px solid var(--color-accent)',
                  display: 'grid',
                  placeItems: 'center',
                  fontFamily: 'var(--font-heading)',
                  fontSize: 13,
                  color: 'var(--color-accent-700)'
                }}
              >
                {iniciales}
              </span>
              <div style={{ lineHeight: 1.2 }}>
                <div style={{ fontSize: 13 }}>{usuario?.nombre}</div>
                <div className="text-muted" style={{ fontSize: 11 }}>
                  {usuario?.rol}
                </div>
              </div>
            </div>
          </div>
        </div>

        <div style={{ height: 'var(--space-6)' }} />

        <Outlet />
      </div>
    </main>
  )
}
