import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import * as productsApi from '../../api/products'
import { API_URL, ApiError } from '../../api/client'
import type { PublicProduct } from '../../types/product'
import { ArDialog } from '../../components/ArDialog'
import { useCart } from '../../cart/CartContext'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

interface VariantOption {
  id?: number
  sku: string
  nombre: string
  tipo: string
  codigoColorHex: string
  precioAjusteCOP: number
  fotoUrl?: string | null
}

const DEFAULT_ACABADOS: VariantOption[] = [
  {
    sku: 'ROB-NAT',
    nombre: 'Roble natural',
    tipo: 'Madera',
    codigoColorHex: '#c9a063',
    precioAjusteCOP: 0
  },
  {
    sku: 'NOG-OSC',
    nombre: 'Nogal oscuro',
    tipo: 'Madera',
    codigoColorHex: '#4a2c11',
    precioAjusteCOP: 0
  },
  {
    sku: 'LIN-CRU',
    nombre: 'Lino crudo',
    tipo: 'Tela',
    codigoColorHex: '#ded7cb',
    precioAjusteCOP: 80000
  },
  {
    sku: 'GRP-PIE',
    nombre: 'Gris piedra',
    tipo: 'Tela',
    codigoColorHex: '#7d7979',
    precioAjusteCOP: 80000
  }
]

export function ProductoPage() {
  const { slug = '' } = useParams<{ slug: string }>()
  const [producto, setProducto] = useState<PublicProduct | null>(null)
  const [relacionados, setRelacionados] = useState<PublicProduct[]>([])
  const [cargando, setCargando] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [variante, setVariante] = useState<VariantOption>(DEFAULT_ACABADOS[0])
  const [agregado, setAgregado] = useState(false)
  const [arAbierto, setArAbierto] = useState(false)
  const { agregar } = useCart()

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
        if (p.variantes && p.variantes.length > 0) {
          setVariante({
            id: p.variantes[0].id,
            sku: p.variantes[0].sku,
            nombre: p.variantes[0].nombre,
            tipo: p.variantes[0].tipo,
            codigoColorHex: p.variantes[0].codigoColorHex || '#c9a063',
            precioAjusteCOP: p.variantes[0].precioAjusteCOP || 0,
            fotoUrl: p.variantes[0].fotoUrl
          })
        } else {
          setVariante(DEFAULT_ACABADOS[0])
        }
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

  const variantesDisponibles: VariantOption[] = (producto.variantes && producto.variantes.length > 0)
    ? producto.variantes.map((v) => ({
        id: v.id,
        sku: v.sku,
        nombre: v.nombre,
        tipo: v.tipo,
        codigoColorHex: v.codigoColorHex || '#a18262',
        precioAjusteCOP: v.precioAjusteCOP || 0,
        fotoUrl: v.fotoUrl
      }))
    : DEFAULT_ACABADOS

  const precioFinal = producto.precioCOP + (variante?.precioAjusteCOP ?? 0)
  const cuota = Math.round(precioFinal / 12)
  const imagenPrincipal = variante?.fotoUrl
    ? absolutoImg(variante.fotoUrl)
    : (producto.imagenUrl ? absolutoImg(producto.imagenUrl) : null)

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
              position: 'relative',
              display: 'grid',
              placeItems: 'center',
              borderRadius: 'var(--radius-md)',
              overflow: 'hidden',
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
            <div
              style={{
                position: 'absolute',
                bottom: 12,
                left: 12,
                display: 'flex',
                alignItems: 'center',
                gap: 6,
                padding: '4px 10px',
                background: 'rgba(255, 255, 255, 0.92)',
                backdropFilter: 'blur(6px)',
                borderRadius: 'var(--radius-sm, 6px)',
                boxShadow: '0 2px 6px rgba(0,0,0,0.08)',
                fontSize: 11,
                color: 'var(--color-text-main, #242220)'
              }}
            >
              <span
                style={{
                  width: 10,
                  height: 10,
                  borderRadius: '50%',
                  backgroundColor: variante.codigoColorHex,
                  border: '1px solid rgba(0,0,0,0.2)'
                }}
              />
              <span>
                <strong>{variante.nombre}</strong> ({variante.sku})
              </span>
            </div>
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
              {monedaCOP.format(precioFinal)}
            </span>
            {variante.precioAjusteCOP > 0 && (
              <span className="tag tag-accent" style={{ fontSize: 11 }}>
                +{monedaCOP.format(variante.precioAjusteCOP)} acabado seleccionado
              </span>
            )}
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

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 'var(--space-3)' }}>
            <h6 style={{ margin: 0 }}>Variante de Acabado</h6>
            <span style={{ fontSize: 12, color: 'var(--color-accent-700)', fontWeight: 500 }}>
              {variante.nombre} · {variante.tipo} (SKU: {variante.sku})
            </span>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(135px, 1fr))', gap: 'var(--space-2)' }}>
            {variantesDisponibles.map((v) => {
              const seleccionada = variante.sku === v.sku
              return (
                <button
                  key={v.sku}
                  type="button"
                  onClick={() => setVariante(v)}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 'var(--space-2)',
                    padding: '8px 12px',
                    border: seleccionada ? '2px solid var(--color-accent-700)' : '1px solid var(--color-divider)',
                    borderRadius: 'var(--radius-md)',
                    background: seleccionada ? 'var(--color-surface-sunken, #f8f6f3)' : '#ffffff',
                    cursor: 'pointer',
                    textAlign: 'left',
                    transition: 'all 0.15s ease'
                  }}
                >
                  <span
                    style={{
                      width: 18,
                      height: 18,
                      borderRadius: '50%',
                      backgroundColor: v.codigoColorHex,
                      border: '1px solid rgba(0,0,0,0.15)',
                      flexShrink: 0,
                      boxShadow: seleccionada ? '0 0 0 2px #ffffff, 0 0 0 3px var(--color-accent-700)' : 'none'
                    }}
                  />
                  <div style={{ minWidth: 0 }}>
                    <div style={{ fontSize: 12, fontWeight: seleccionada ? 600 : 400, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {v.nombre}
                    </div>
                    <div style={{ fontSize: 10, color: 'var(--color-text-muted)' }}>
                      {v.precioAjusteCOP > 0 ? `+${monedaCOP.format(v.precioAjusteCOP)}` : 'Base'}
                    </div>
                  </div>
                </button>
              )
            })}
          </div>

          <div style={{ display: 'flex', gap: 'var(--space-3)', marginTop: 'var(--space-6)', flexWrap: 'wrap' }}>
            <button
              className="btn btn-primary"
              onClick={() => {
                agregar({
                  productId: producto.id,
                  slug: producto.slug,
                  nombre: producto.nombre,
                  imagenUrl: producto.imagenUrl,
                  precioBase: producto.precioCOP,
                  cantidad: 1,
                  variante: variante.id
                    ? {
                        id: variante.id,
                        nombre: variante.nombre,
                        tipo: variante.tipo,
                        codigoColorHex: variante.codigoColorHex,
                        ajusteCOP: variante.precioAjusteCOP
                      }
                    : null
                })
                setAgregado(true)
              }}
              style={{ fontSize: 15, padding: '12px 26px' }}
              disabled={producto.estado !== 'Disponible'}
            >
              {producto.estado === 'Disponible' ? 'Agregar al carrito' : 'No disponible'}
            </button>
            <button
              className="btn btn-secondary"
              style={{ fontSize: 15, padding: '12px 22px' }}
              onClick={() => setArAbierto(true)}
            >
              Ver en tu espacio
            </button>
            {agregado && (
              <span className="tag tag-accent" style={{ alignSelf: 'center' }}>
                Agregado: {producto.nombre} · {variante.nombre} ({variante.sku})
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
      <ArDialog abierto={arAbierto} alCerrar={() => setArAbierto(false)} producto={producto.nombre} modelo3dUrl={producto.modelo3dUrl} modeloUsdzUrl={producto.modeloUsdzUrl} />
    </main>
  )
}

function absolutoImg(url: string): string {
  if (/^https?:/i.test(url)) return url
  const base = API_URL.replace(/\/api\/?$/, '')
  return `${base}${url.startsWith('/') ? '' : '/'}${url}`
}
