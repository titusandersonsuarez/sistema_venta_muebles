import { Link, NavLink, Outlet } from 'react-router-dom'

export function StoreLayout() {
  return (
    <div style={{ background: 'var(--color-bg)', color: 'var(--color-text)', minHeight: '100vh' }}>
      <header
        className="nav"
        style={{
          position: 'sticky',
          top: 0,
          zIndex: 20,
          background: 'var(--color-bg)',
          flexWrap: 'wrap',
          rowGap: 'var(--space-2)',
          gap: 'var(--space-3)',
          paddingLeft: 'max(var(--space-4),4vw)',
          paddingRight: 'max(var(--space-4),4vw)'
        }}
      >
        <Link
          to="/"
          className="nav-brand"
          style={{ textDecoration: 'none', letterSpacing: '0.02em' }}
        >
          Nogal
        </Link>
        <nav style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-4)', flexWrap: 'wrap' }}>
          <NavLink to="/" end>Inicio</NavLink>
          <NavLink to="/catalogo">Catálogo</NavLink>
          <NavLink to="/fabrica">La fábrica</NavLink>
          <NavLink to="/admin">Admin</NavLink>
          <span style={{ width: 1, height: 18, background: 'var(--color-divider)' }} />
          <Link
            to="/catalogo"
            className="btn btn-secondary"
            style={{ fontSize: 13, textDecoration: 'none' }}
          >
            Carrito · 0
          </Link>
        </nav>
      </header>

      <Outlet />
    </div>
  )
}
