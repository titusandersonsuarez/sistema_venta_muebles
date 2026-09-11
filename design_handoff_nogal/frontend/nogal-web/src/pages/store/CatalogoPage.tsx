import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import * as productsApi from '../../api/products'
import { API_URL } from '../../api/client'
import type { CatalogOptions, PublicProduct } from '../../types/product'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

const PRECIO_MIN = 150000
const PRECIO_MAX = 2500000
const PRECIO_STEP = 50000

const TODOS = 'Todos'

export function CatalogoPage() {
  const [opciones, setOpciones] = useState<CatalogOptions | null>(null)
  const [productos, setProductos] = useState<PublicProduct[]>([])
  const [total, setTotal] = useState(0)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [categoria, setCategoria] = useState(TODOS)
  const [material, setMaterial] = useState(TODOS)
  const [precioMax, setPrecioMax] = useState(PRECIO_MAX)

  useEffect(() => {
    let cancelado = false
    ;(async () => {
      try {
        const opts = await productsApi.obtenerOpciones()
        if (!cancelado) setOpciones(opts)
      } catch {
        if (!cancelado) setError('No pudimos cargar las opciones del catálogo.')
      }
    })()
    return () => {
      cancelado = true
    }
  }, [])

  useEffect(() => {
    let cancelado = false
    setCargando(true)
    ;(async () => {
      try {
        const page = await productsApi.listarPublico({
          categoria: categoria === TODOS ? undefined : categoria,
          material: material === TODOS ? undefined : material,
          precioMax: precioMax < PRECIO_MAX ? precioMax : undefined,
          pagina: 1,
          tamano: 60
        })
        if (cancelado) return
        setProductos(page.items)
        setTotal(page.total)
        setError(null)
      } catch {
        if (!cancelado) setError('No pudimos cargar el catálogo. Intenta de nuevo en un momento.')
      } finally {
        if (!cancelado) setCargando(false)
      }
    })()
    return () => {
      cancelado = true
    }
  }, [categoria, material, precioMax])

  function limpiar() {
    setCategoria(TODOS)
    setMaterial(TODOS)
    setPrecioMax(PRECIO_MAX)
  }

  const categorias = useMemo(() => [TODOS, ...(opciones?.categorias ?? [])], [opciones])
  const materiales = useMemo(() => [TODOS, ...(opciones?.materiales ?? [])], [opciones])

  return (
    <main
      style={{
        padding: 'var(--space-8) max(var(--space-4),4vw) calc(var(--space-8) * 1.5)'
      }}
    >
      <p className="text-muted" style={{ fontSize: 12 }}>
        <Link to="/">Inicio</Link> / Catálogo
      </p>
      <h1 style={{ fontWeight: 400, fontSize: 44, marginBottom: 'var(--space-2)' }}>Catálogo</h1>
      <p className="text-muted" style={{ fontSize: 14, marginBottom: 'var(--space-6)' }}>
        {cargando
          ? 'Cargando…'
          : `${total} mueble${total === 1 ? '' : 's'} fabricado${total === 1 ? '' : 's'} en Bogotá, listo${total === 1 ? '' : 's'} para despacho.`}
      </p>
      <hr className="hr" />

      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--space-8)', alignItems: 'flex-start' }}>
        <aside
          style={{
            flex: '1 1 200px',
            maxWidth: 240,
            display: 'flex',
            flexDirection: 'column',
            gap: 'var(--space-6)'
          }}
        >
          <div>
            <h6 style={{ marginBottom: 'var(--space-3)' }}>Categoría</h6>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
              {categorias.map((c) => (
                <label key={c} className="radio">
                  <input
                    type="radio"
                    name="cat"
                    value={c}
                    checked={categoria === c}
                    onChange={(e) => setCategoria(e.target.value)}
                  />
                  <span className="dot" />
                  {c}
                </label>
              ))}
            </div>
          </div>

          <hr className="hr" style={{ margin: 0 }} />

          <div>
            <h6 style={{ marginBottom: 'var(--space-3)' }}>Precio máximo</h6>
            <input
              className="input"
              type="range"
              min={PRECIO_MIN}
              max={PRECIO_MAX}
              step={PRECIO_STEP}
              value={precioMax}
              onChange={(e) => setPrecioMax(Number(e.target.value))}
              style={{ padding: 0, border: 0, accentColor: 'var(--color-accent)' }}
            />
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                fontSize: 12,
                fontVariantNumeric: 'tabular-nums',
                marginTop: 4,
                gap: 'var(--space-2)'
              }}
            >
              <span className="text-muted">{monedaCOP.format(PRECIO_MIN)}</span>
              <span>hasta {monedaCOP.format(precioMax)}</span>
            </div>
          </div>

          <hr className="hr" style={{ margin: 0 }} />

          <div>
            <h6 style={{ marginBottom: 'var(--space-3)' }}>Material</h6>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
              {materiales.map((m) => (
                <label key={m} className="radio">
                  <input
                    type="radio"
                    name="mat"
                    value={m}
                    checked={material === m}
                    onChange={(e) => setMaterial(e.target.value)}
                  />
                  <span className="dot" />
                  {m}
                </label>
              ))}
            </div>
          </div>

          <hr className="hr" style={{ margin: 0 }} />

          <button className="btn btn-ghost" onClick={limpiar} style={{ alignSelf: 'flex-start' }}>
            Limpiar filtros
          </button>
        </aside>

        <div style={{ flex: '100 1 260px', minWidth: 0 }}>
          {error && (
            <p role="alert" style={{ color: 'var(--color-accent-700)', fontSize: 13 }}>
              {error}
            </p>
          )}

          {!cargando && productos.length === 0 && !error ? (
            <div
              style={{
                border: '1px solid var(--color-divider)',
                borderRadius: 'var(--radius-md)',
                padding: 'var(--space-8)',
                textAlign: 'center'
              }}
            >
              <h3 style={{ fontWeight: 400 }}>Nada con esos filtros</h3>
              <p className="text-muted" style={{ fontSize: 14 }}>
                Prueba a subir el precio máximo o a cambiar el material.
              </p>
              <button className="btn btn-primary" onClick={limpiar}>Limpiar filtros</button>
            </div>
          ) : (
            <div
              style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))',
                gap: 'var(--space-6)'
              }}
            >
              {productos.map((p) => (
                <Link
                  key={p.id}
                  to={`/producto/${p.slug}`}
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
                      aspectRatio: '4 / 3.4',
                      display: 'grid',
                      placeItems: 'center',
                      borderRadius: 'var(--radius-md)',
                      background: p.imagenUrl
                        ? `center/cover url("${absolutoImg(p.imagenUrl)}")`
                        : 'repeating-linear-gradient(135deg,#eae9e9 0 10px,#e1dedb 10px 20px)'
                    }}
                  >
                    {!p.imagenUrl && (
                      <span
                        style={{
                          fontFamily: 'ui-monospace, SFMono-Regular, Menlo, monospace',
                          fontSize: 10,
                          letterSpacing: '0.08em',
                          color: '#7d7979'
                        }}
                      >
                        {p.nombre}
                      </span>
                    )}
                  </div>
                  <div
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      gap: 'var(--space-2)',
                      alignItems: 'baseline'
                    }}
                  >
                    <span style={{ fontFamily: 'var(--font-heading)', fontSize: 18 }}>{p.nombre}</span>
                    <span
                      style={{
                        fontVariantNumeric: 'tabular-nums',
                        fontSize: 15,
                        whiteSpace: 'nowrap'
                      }}
                    >
                      {monedaCOP.format(p.precioCOP)}
                    </span>
                  </div>
                  <div className="text-muted" style={{ fontSize: 12 }}>
                    {p.material}
                    {p.medidas ? ` · ${p.medidas}` : ''}
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </main>
  )
}

function absolutoImg(url: string): string {
  if (/^https?:/i.test(url)) return url
  const base = API_URL.replace(/\/api\/?$/, '')
  return `${base}${url.startsWith('/') ? '' : '/'}${url}`
}
