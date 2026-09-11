import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import * as productsApi from '../../api/products'
import { API_URL, ApiError } from '../../api/client'
import type { PublicProduct } from '../../types/product'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

const ACABADOS = ['Roble natural', 'Nogal oscuro', 'Lino crudo', 'Gris piedra']

export function ProductoPage() {
  const { slug = '' } = useParams<{ slug: string }>()
  const [producto, setProducto] = useState<PublicProduct | null>(null)
  const [relacionados, setRelacionados] = useState<PublicProduct[]>([])
  const [cargando, setCargando] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [acabado, setAcabado] = useState(ACABADOS[0])
  const [agregado, setAgregado] = useState(false)

  useEffect(() => {
    let cancelado = false
    setCargando(true)
    setNotFound(false)
    setAgregado(false)
    ;(async () => {
      try {
        const p = await productsApi.obtenerPorSlug(slug)
        if (cancelado) return
        setProducto(p)
        const otros = await productsApi.listarPublico({ tamano: 20 })
        if (cancelado) return
        setRelacionados(
          otros.items.filter((x) => x.slug !== slug && x.categoria !== p.categoria).slice(0, 3)
        )
      } catch (err) {
        if (cancelado) return
        if (err instanceof ApiError && err.status === 404) {
          setNotFound(true)
        } else {
          setNotFound(true)
        }
      } finally {
        if (!cancelado) {
          setCargando(false)
          window.scrollTo(0, 0)
        }
      }
    })()
    return () => {
      cancelado = true
    }
  }, [slug])

  if (cargando) {
    return (
      <main style={{ padding: 'var(--space-8) max(var(--space-4),4vw)' }}>
        <p className="text-muted">Cargando producto…</p>
      </main>
    )
  }

  if (notFound || !producto) {
    return (
      <main style={{ padding: 'var(--space-8) max(var(--space-4),4vw)' }}>
        <p className="text-muted" style={{ fontSize: 12, marginBottom: 'var(--space-6)' }}>
          <Link to="/">Inicio</Link> / <Link to="/catalogo">Catálogo</Link>
        </p>
        <div
          style={{
            border: '1px solid var(--color-divider)',
            borderRadius: 'var(--radius-md)',
            padding: 'var(--space-8)',
            textAlign: 'center'
          }}
        >
          <h3 style={{ fontWeight: 400 }}>No encontramos ese mueble</h3>
          <p className="text-muted" style={{ fontSize: 14 }}>
            Puede que ya no esté disponible. Mira los que sí están en el catálogo.
          </p>
          <Link to="/catalogo" className="btn btn-primary">Ver el catálogo</Link>
        </div>
      </main>
    )
  }

  const cuota = Math.round(producto.precioCOP / 12)
  const imagenPrincipal = producto.imagenUrl ? absolutoImg(producto.imagenUrl) : null

  return (
    <main
      style={{
        padding: 'var(--space-8) max(var(--space-4),4vw) calc(var(--space-8) * 1.5)'
      }}
    >
      <p className="text-muted" style={{ fontSize: 12, marginBottom: 'var(--space-6)' }}>
        <Link to="/">Inicio</Link> / <Link to="/catalogo">Catálogo</Link> / {producto.nombre}
      </p>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
          gap: 'calc(var(--space-8) * 1.2)',
          alignItems: 'start'
        }}
      >
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-3)' }}>
          <div
            className="plate"
            style={{
              aspectRatio: '4 / 3.2',
              display: 'grid',
              placeItems: 'center',
              borderRadius: 'var(--radius-md)',
              background: imagenPrincipal
                ? `center/cover url("${imagenPrincipal}")`
                : 'repeating-linear-gradient(135deg,#eae9e9 0 12px,#e1dedb 12px 24px)'
            }}
          >
            {!imagenPrincipal && (
              <span
                style={{
                  fontFamily: 'ui-monospace, SFMono-Regular, Menlo, monospace',
                  fontSize: 11,
                  letterSpacing: '0.08em',
                  color: '#7d7979'
                }}
              >
                {producto.nombre} — foto principal
              </span>
            )}
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 'var(--space-3)' }}>
            {[0, 1, 2].map((i) => {
              const extra = producto.imagenes[i]
              return (
                <div
                  key={i}
                  className="plate"
                  style={{
                    aspectRatio: '1 / 1',
                    display: 'grid',
                    placeItems: 'center',
                    borderRadius: 'var(--radius-md)',
                    background: extra
                      ? `center/cover url("${absolutoImg(extra.url)}")`
                      : 'repeating-linear-gradient(135deg,#eae9e9 0 8px,#e1dedb 8px 16px)'
                  }}
                >
                  {!extra && (
                    <span
                      style={{
                        fontFamily: 'ui-monospace, SFMono-Regular, Menlo, monospace',
                        fontSize: 9,
                        letterSpacing: '0.06em',
                        color: '#7d7979'
                      }}
                    >
                      detalle {i + 1}
                    </span>
                  )}
                </div>
              )
            })}
          </div>
        </div>

        <div style={{ maxWidth: '34em' }}>
          <p
            style={{
              fontSize: 11,
              letterSpacing: '0.14em',
              textTransform: 'uppercase',
              color: 'var(--color-accent-700)'
            }}
          >
            {producto.categoria} · hecho en nuestro taller
          </p>
          <h1
            style={{
              fontWeight: 400,
              fontSize: 'clamp(34px, 4vw, 48px)',
              margin: 'var(--space-2) 0'
            }}
          >
            {producto.nombre}
          </h1>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 'var(--space-3)', flexWrap: 'wrap' }}>
            <span
              style={{
                fontFamily: 'var(--font-heading)',
                fontSize: 34,
                fontVariantNumeric: 'tabular-nums'
              }}
            >
              {monedaCOP.format(producto.precioCOP)}
            </span>
            <span className="text-muted" style={{ fontSize: 13 }}>
              IVA incluido · o {monedaCOP.format(cuota)} al mes en 12 cuotas
            </span>
          </div>

          {producto.descripcion && (
            <p style={{ marginTop: 'var(--space-4)', textAlign: 'justify', lineHeight: 1.75 }}>
              {producto.descripcion}
            </p>
          )}

          <hr className="hr" />

          <h6 style={{ marginBottom: 'var(--space-3)' }}>Acabado</h6>
          <div className="seg" style={{ flexWrap: 'wrap' }}>
            {ACABADOS.map((a) => (
              <label key={a} className="seg-opt">
                <input
                  type="radio"
                  name="acabado"
                  value={a}
                  checked={acabado === a}
                  onChange={(e) => setAcabado(e.target.value)}
                />
                {a}
              </label>
            ))}
          </div>

          <div style={{ display: 'flex', gap: 'var(--space-3)', marginTop: 'var(--space-6)', flexWrap: 'wrap' }}>
            <button
              className="btn btn-primary"
              onClick={() => setAgregado(true)}
              style={{ fontSize: 15, padding: '12px 26px' }}
              disabled={producto.estado !== 'Disponible'}
            >
              {producto.estado === 'Disponible' ? 'Agregar al carrito' : 'No disponible'}
            </button>
            <button
              className="btn btn-secondary"
              style={{ fontSize: 15, padding: '12px 22px' }}
              onClick={() => alert('El módulo AR está en construcción.')}
            >
              Ver en tu espacio
            </button>
            {agregado && (
              <span className="tag tag-accent" style={{ alignSelf: 'center' }}>
                Agregado con acabado {acabado}
              </span>
            )}
          </div>

          <p className="text-muted" style={{ fontSize: 12, marginTop: 'var(--space-3)' }}>
            Entrega en 5 días hábiles · Envío gratis desde {monedaCOP.format(500000)} · Devolución en 30 días
          </p>

          <hr className="hr" />
          <h4 style={{ fontWeight: 400, marginBottom: 'var(--space-2)' }}>Medidas y materiales</h4>
          <table className="table">
            <tbody>
              <tr>
                <td className="text-muted">Medidas</td>
                <td style={{ fontVariantNumeric: 'tabular-nums' }}>{producto.medidas ?? '—'}</td>
              </tr>
              <tr>
                <td className="text-muted">Material</td>
                <td>{producto.material}</td>
              </tr>
              <tr>
                <td className="text-muted">Peso</td>
                <td style={{ fontVariantNumeric: 'tabular-nums' }}>{producto.peso ?? '—'}</td>
              </tr>
              <tr>
                <td className="text-muted">Armado</td>
                <td>{producto.armado ?? 'Llega armado'}</td>
              </tr>
              <tr>
                <td className="text-muted">Garantía</td>
                <td>10 años de fábrica</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      {relacionados.length > 0 && (
        <section style={{ marginTop: 'calc(var(--space-8) * 1.6)' }}>
          <hr className="hr" />
          <h2 style={{ fontWeight: 400, marginBottom: 'var(--space-6)' }}>Combina bien con</h2>
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(230px, 1fr))',
              gap: 'var(--space-6)'
            }}
          >
            {relacionados.map((r) => (
              <Link
                key={r.id}
                to={`/producto/${r.slug}`}
                style={{
                  textDecoration: 'none',
                  color: 'inherit',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 'var(--space-2)'
                }}
              >
                <div
                  className="plate"
                  style={{
                    aspectRatio: '4 / 3.2',
                    borderRadius: 'var(--radius-md)',
                    background: r.imagenUrl
                      ? `center/cover url("${absolutoImg(r.imagenUrl)}")`
                      : 'repeating-linear-gradient(135deg,#eae9e9 0 10px,#e1dedb 10px 20px)'
                  }}
                />
                <div
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'baseline',
                    gap: 'var(--space-2)'
                  }}
                >
                  <span style={{ fontFamily: 'var(--font-heading)', fontSize: 17 }}>{r.nombre}</span>
                  <span
                    style={{
                      fontVariantNumeric: 'tabular-nums',
                      fontSize: 14,
                      whiteSpace: 'nowrap'
                    }}
                  >
                    {monedaCOP.format(r.precioCOP)}
                  </span>
                </div>
                <div className="text-muted" style={{ fontSize: 12 }}>{r.categoria}</div>
              </Link>
            ))}
          </div>
        </section>
      )}
    </main>
  )
}

function absolutoImg(url: string): string {
  if (/^https?:/i.test(url)) return url
  const base = API_URL.replace(/\/api\/?$/, '')
  return `${base}${url.startsWith('/') ? '' : '/'}${url}`
}
