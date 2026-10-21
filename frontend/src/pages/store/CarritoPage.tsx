import { useMemo, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useCart } from '../../cart/CartContext'
import { API_URL, ApiError } from '../../api/client'
import * as ordersApi from '../../api/orders'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

const UMBRAL_ENVIO_GRATIS = 500_000
const COSTO_ENVIO_BASE = 30_000

export function CarritoPage() {
  const { items, subtotalCOP, cantidad, quitar, setCantidad, vaciar } = useCart()
  const [cliente, setCliente] = useState('')
  const [ciudad, setCiudad] = useState('')
  const [contacto, setContacto] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const envio = useMemo(
    () => (items.length === 0 || subtotalCOP >= UMBRAL_ENVIO_GRATIS ? 0 : COSTO_ENVIO_BASE),
    [items.length, subtotalCOP]
  )
  const total = subtotalCOP + envio

  async function confirmar(e: FormEvent) {
    e.preventDefault()
    setError(null)
    if (items.length === 0) return
    setEnviando(true)
    try {
      const creado = await ordersApi.crear({
        cliente: cliente.trim(),
        ciudad: ciudad.trim(),
        contacto: contacto.trim(),
        items: items.map(i => ({
          productId: i.productId,
          productVariantId: i.variante?.id ?? null,
          cantidad: i.cantidad
        }))
      })
      // Iniciamos la intención de pago y redirigimos a la pasarela.
      // En modo demo, checkoutUrl apunta a nuestra pantalla /carrito/pago-demo/:codigo.
      const intencion = await ordersApi.iniciarPago(creado.codigo)
      vaciar()
      window.location.href = intencion.checkoutUrl
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
      } else {
        setError('No se pudo crear el pedido. Revisa tu conexión.')
      }
      setEnviando(false)
    }
  }

  if (items.length === 0) {
    return (
      <main style={{ maxWidth: 720, margin: '0 auto', padding: 'var(--space-6) max(var(--space-4), 4vw)' }}>
        <p style={{ fontSize: 13, color: 'var(--color-text-muted)', marginBottom: 'var(--space-2)' }}>
          <Link to="/">Inicio</Link> · Carrito
        </p>
        <h1 style={{ margin: 0, marginBottom: 'var(--space-3)' }}>Tu carrito está vacío</h1>
        <p style={{ marginBottom: 'var(--space-5)' }}>
          Aún no has agregado muebles. Ve al catálogo, elige el acabado y vuelve por aquí.
        </p>
        <Link to="/catalogo" className="btn btn-primary" style={{ textDecoration: 'none' }}>
          Ver el catálogo
        </Link>
      </main>
    )
  }

  return (
    <main style={{ padding: 'var(--space-6) max(var(--space-4), 4vw)' }}>
      <p style={{ fontSize: 13, color: 'var(--color-text-muted)', marginBottom: 'var(--space-2)' }}>
        <Link to="/">Inicio</Link> · Carrito
      </p>
      <h1 style={{ margin: 0, marginBottom: 'var(--space-2)' }}>Tu carrito</h1>
      <p style={{ marginBottom: 'var(--space-5)', color: 'var(--color-text-muted)' }}>
        {cantidad} {cantidad === 1 ? 'mueble listo' : 'muebles listos'} para despacho desde nuestra fábrica en Bogotá.
      </p>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
          gap: 'var(--space-8)',
          alignItems: 'start'
        }}
      >
        <section>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
            {items.map(item => {
              const precioUnitario = item.precioBase + (item.variante?.ajusteCOP ?? 0)
              const subtotalLinea = precioUnitario * item.cantidad
              return (
                <article
                  key={`${item.productId}::${item.variante?.id ?? 'base'}`}
                  className="card"
                  style={{
                    display: 'grid',
                    gridTemplateColumns: '96px 1fr auto',
                    gap: 'var(--space-4)',
                    alignItems: 'center',
                    padding: 'var(--space-3)'
                  }}
                >
                  <div
                    className="plate"
                    style={{
                      aspectRatio: '1 / 1',
                      background: item.imagenUrl
                        ? `center/cover url("${absolutoImg(item.imagenUrl)}")`
                        : 'var(--color-surface)'
                    }}
                  />
                  <div style={{ minWidth: 0 }}>
                    <Link
                      to={`/producto/${item.slug}`}
                      style={{ textDecoration: 'none', color: 'var(--color-text)' }}
                    >
                      <h3 style={{ margin: 0, fontSize: 18 }}>{item.nombre}</h3>
                    </Link>
                    {item.variante && (
                      <div style={{ display: 'flex', gap: 'var(--space-2)', alignItems: 'center', marginTop: 4 }}>
                        {item.variante.codigoColorHex && (
                          <span
                            aria-hidden
                            style={{
                              width: 12,
                              height: 12,
                              borderRadius: '50%',
                              background: item.variante.codigoColorHex,
                              border: '1px solid var(--color-divider)'
                            }}
                          />
                        )}
                        <span style={{ fontSize: 13, color: 'var(--color-text-muted)' }}>
                          {item.variante.nombre}
                          {item.variante.ajusteCOP > 0 && ` · +${monedaCOP.format(item.variante.ajusteCOP)}`}
                        </span>
                      </div>
                    )}
                    <div
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 'var(--space-3)',
                        marginTop: 'var(--space-2)'
                      }}
                    >
                      <div style={{ display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                        <button
                          type="button"
                          className="btn btn-ghost"
                          style={{ padding: '2px 10px' }}
                          onClick={() =>
                            setCantidad(item.productId, item.variante?.id ?? null, item.cantidad - 1)
                          }
                          aria-label="Disminuir cantidad"
                        >
                          −
                        </button>
                        <span
                          style={{
                            minWidth: 24,
                            textAlign: 'center',
                            fontVariantNumeric: 'tabular-nums'
                          }}
                        >
                          {item.cantidad}
                        </span>
                        <button
                          type="button"
                          className="btn btn-ghost"
                          style={{ padding: '2px 10px' }}
                          onClick={() =>
                            setCantidad(item.productId, item.variante?.id ?? null, item.cantidad + 1)
                          }
                          aria-label="Aumentar cantidad"
                        >
                          +
                        </button>
                      </div>
                      <button
                        type="button"
                        className="btn btn-ghost"
                        style={{ fontSize: 12, color: 'var(--color-text-muted)' }}
                        onClick={() => quitar(item.productId, item.variante?.id ?? null)}
                      >
                        Quitar
                      </button>
                    </div>
                  </div>
                  <div style={{ textAlign: 'right', fontVariantNumeric: 'tabular-nums' }}>
                    <div style={{ fontSize: 17 }}>{monedaCOP.format(subtotalLinea)}</div>
                    {item.cantidad > 1 && (
                      <div style={{ fontSize: 12, color: 'var(--color-text-muted)' }}>
                        {monedaCOP.format(precioUnitario)} c/u
                      </div>
                    )}
                  </div>
                </article>
              )
            })}
          </div>
        </section>

        <section>
          <div className="card" style={{ padding: 'var(--space-5)' }}>
            <h2 style={{ marginTop: 0, marginBottom: 'var(--space-4)' }}>Resumen</h2>
            <dl
              style={{
                display: 'grid',
                gridTemplateColumns: '1fr auto',
                rowGap: 'var(--space-2)',
                columnGap: 'var(--space-3)',
                margin: 0,
                fontVariantNumeric: 'tabular-nums'
              }}
            >
              <dt>Subtotal</dt>
              <dd style={{ margin: 0, textAlign: 'right' }}>{monedaCOP.format(subtotalCOP)}</dd>
              <dt>Envío</dt>
              <dd style={{ margin: 0, textAlign: 'right' }}>
                {envio === 0 ? 'Gratis' : monedaCOP.format(envio)}
              </dd>
            </dl>
            {envio > 0 && (
              <p style={{ fontSize: 12, color: 'var(--color-text-muted)', marginTop: 'var(--space-2)' }}>
                Envío gratis en pedidos desde {monedaCOP.format(UMBRAL_ENVIO_GRATIS)}.
              </p>
            )}
            <hr className="hr" style={{ margin: 'var(--space-4) 0' }} />
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'baseline',
                fontVariantNumeric: 'tabular-nums'
              }}
            >
              <span style={{ fontSize: 16 }}>Total</span>
              <strong style={{ fontSize: 22 }}>{monedaCOP.format(total)}</strong>
            </div>

            <form onSubmit={confirmar} style={{ marginTop: 'var(--space-5)' }}>
              <div className="field">
                <label htmlFor="carrito-nombre">Nombre</label>
                <input
                  id="carrito-nombre"
                  className="input"
                  value={cliente}
                  onChange={e => setCliente(e.target.value)}
                  required
                  maxLength={120}
                  autoComplete="name"
                />
              </div>
              <div className="field">
                <label htmlFor="carrito-ciudad">Ciudad</label>
                <input
                  id="carrito-ciudad"
                  className="input"
                  value={ciudad}
                  onChange={e => setCiudad(e.target.value)}
                  required
                  maxLength={80}
                  autoComplete="address-level2"
                  placeholder="Bogotá"
                />
              </div>
              <div className="field">
                <label htmlFor="carrito-contacto">WhatsApp o correo</label>
                <input
                  id="carrito-contacto"
                  className="input"
                  value={contacto}
                  onChange={e => setContacto(e.target.value)}
                  required
                  maxLength={120}
                  placeholder="300 000 0000"
                />
              </div>
              {error && (
                <p
                  role="alert"
                  style={{
                    color: 'var(--color-accent-700)',
                    borderLeft: '2px solid var(--color-accent)',
                    paddingLeft: 'var(--space-2)',
                    marginBottom: 'var(--space-3)',
                    fontSize: 13
                  }}
                >
                  {error}
                </p>
              )}
              <button
                type="submit"
                className="btn btn-primary btn-block"
                disabled={enviando}
                style={{ fontSize: 15 }}
              >
                {enviando ? 'Preparando el pago…' : 'Ir a pagar'}
              </button>
              <p style={{ fontSize: 12, color: 'var(--color-text-muted)', marginTop: 'var(--space-3)' }}>
                Te llevamos a la pasarela para pagar con tarjeta, PSE, Nequi o Bancolombia. Al confirmar el pago
                regresas al taller con la confirmación de tu pedido.
              </p>
            </form>
          </div>
        </section>
      </div>
    </main>
  )
}

function absolutoImg(url: string): string {
  if (/^https?:/i.test(url)) return url
  const base = API_URL.replace(/\/api\/?$/, '')
  return `${base}${url.startsWith('/') ? '' : '/'}${url}`
}
