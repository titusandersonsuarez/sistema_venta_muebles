import { Link, NavLink, Outlet } from 'react-router-dom'
import { NogalitoChat } from '../components/NogalitoChat'
import { useCart } from '../cart/CartContext'

export function StoreLayout() {
  const { cantidad } = useCart()
  return (
    <div style={{ background: 'var(--color-bg)', color: 'var(--color-text)', minHeight: '100vh', position: 'relative' }}>
      <header
        className="nav"
        style={{
          position: 'sticky',
          top: 0,
          zIndex: 20,
          background: 'var(--color-bg)',
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
        <nav className="store-nav-links" style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-4)' }}>
          <NavLink to="/" end className="nav-optional">Inicio</NavLink>
          <NavLink to="/catalogo">Catálogo</NavLink>
          <a href="/#fabrica" className="nav-optional">La fábrica</a>
          <NavLink to="/admin" className="nav-optional">Admin</NavLink>
          <span className="nav-optional" style={{ width: 1, height: 18, background: 'var(--color-divider)' }} />
          <Link
            to="/carrito"
            className="btn btn-secondary"
            style={{ fontSize: 13.5, textDecoration: 'none', gap: 8 }}
            aria-label={`Carrito, ${cantidad} ${cantidad === 1 ? 'mueble' : 'muebles'}`}
          >
            Carrito <span className="cart-count">{cantidad}</span>
          </Link>
        </nav>
      </header>

      <Outlet />
      <NogalitoChat />
    </div>
  )
}
