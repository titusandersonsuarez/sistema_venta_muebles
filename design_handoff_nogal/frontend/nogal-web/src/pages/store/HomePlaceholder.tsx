import { Link } from 'react-router-dom'

export function HomePlaceholder() {
  return (
    <main
      style={{
        display: 'grid',
        placeItems: 'center',
        minHeight: '60vh',
        padding: 'var(--space-8)',
        textAlign: 'center'
      }}
    >
      <div>
        <p
          style={{
            fontSize: 11,
            letterSpacing: '0.14em',
            textTransform: 'uppercase',
            color: 'var(--color-accent-700)'
          }}
        >
          Nogal
        </p>
        <h1 style={{ fontWeight: 400 }}>La tienda pública está en construcción.</h1>
        <p className="text-muted">
          Por ahora, entra al <Link to="/admin/login">panel interno</Link>.
        </p>
      </div>
    </main>
  )
}
