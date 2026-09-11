import { Link } from 'react-router-dom'

export function HomePlaceholder() {
  return (
    <main>
      <section
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
          gap: 'var(--space-8)',
          alignItems: 'center',
          padding: 'calc(var(--space-8) * 1.6) max(var(--space-4), 4vw)'
        }}
      >
        <div style={{ maxWidth: '30em' }}>
          <p
            style={{
              fontSize: 11,
              letterSpacing: '0.14em',
              textTransform: 'uppercase',
              color: 'var(--color-accent-700)',
              marginBottom: 'var(--space-3)'
            }}
          >
            Fábrica propia en Bogotá · Entrega en 5 días hábiles
          </p>
          <h1
            style={{
              fontSize: 'clamp(40px, 5.4vw, 64px)',
              fontWeight: 400,
              lineHeight: 1.05,
              marginBottom: 'var(--space-4)'
            }}
          >
            Somos la fábrica,
            <br />
            no el intermediario.
          </h1>
          <p style={{ fontSize: 17, lineHeight: 1.7, maxWidth: '34em', marginBottom: 'var(--space-6)' }}>
            Salas, comedores, alcobas y sillas hechos en nuestro taller de Puente Aranda y despachados
            directo a tu casa. Sin showroom en centro comercial, sin distribuidor: por eso el precio
            es el que es.
          </p>
          <div style={{ display: 'flex', gap: 'var(--space-3)', flexWrap: 'wrap' }}>
            <Link
              to="/catalogo"
              className="btn btn-primary"
              style={{ fontSize: 15, padding: '12px 22px', textDecoration: 'none' }}
            >
              Ver el catálogo
            </Link>
            <Link
              to="/admin/login"
              className="btn btn-secondary"
              style={{ fontSize: 15, padding: '12px 22px', textDecoration: 'none' }}
            >
              Panel interno
            </Link>
          </div>
          <div style={{ display: 'flex', gap: 'var(--space-6)', marginTop: 'var(--space-8)', flexWrap: 'wrap' }}>
            <div>
              <div style={{ fontFamily: 'var(--font-heading)', fontSize: 26, fontVariantNumeric: 'tabular-nums' }}>
                4,8/5
              </div>
              <div className="text-muted" style={{ fontSize: 12 }}>2.140 reseñas</div>
            </div>
            <div>
              <div style={{ fontFamily: 'var(--font-heading)', fontSize: 26, fontVariantNumeric: 'tabular-nums' }}>
                10 años
              </div>
              <div className="text-muted" style={{ fontSize: 12 }}>de garantía de fábrica</div>
            </div>
            <div>
              <div style={{ fontFamily: 'var(--font-heading)', fontSize: 26, fontVariantNumeric: 'tabular-nums' }}>
                Gratis
              </div>
              <div className="text-muted" style={{ fontSize: 12 }}>envío desde $ 500.000</div>
            </div>
          </div>
        </div>
        <figure
          className="plate"
          style={{
            aspectRatio: '4 / 3.2',
            background: 'repeating-linear-gradient(135deg,#eae9e9 0 12px,#e1dedb 12px 24px)',
            display: 'grid',
            placeItems: 'center',
            borderRadius: 'var(--radius-md)'
          }}
        >
          <figcaption
            style={{
              fontFamily: 'ui-monospace, SFMono-Regular, Menlo, monospace',
              fontSize: 11,
              letterSpacing: '0.1em',
              color: '#7d7979',
              margin: 0
            }}
          >
            sala con sofá Liena — foto principal
          </figcaption>
        </figure>
      </section>
    </main>
  )
}
