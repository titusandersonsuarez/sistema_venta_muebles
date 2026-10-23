import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as ordersApi from '../../api/orders'
import { ApiError } from '../../api/client'
import type { Order } from '../../types/order'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

/**
 * Pantalla de simulación cuando Wompi:Provider = "demo". Emula el hosted
 * checkout de la pasarela: muestra el resumen del pedido y dos botones
 * (Aprobar / Rechazar). Al pulsar, marca el pedido como pagado o
 * rechazado en el backend y regresa a /carrito/gracias/:codigo.
 */
export function PagoDemoPage() {
  const { codigo = '' } = useParams<{ codigo: string }>()
  const [pedido, setPedido] = useState<Order | null>(null)
  const [cargando, setCargando] = useState(true)
  const [procesando, setProcesando] = useState<'aprobado' | 'rechazado' | null>(null)
  const [error, setError] = useState<string | null>(null)
  const navigate = useNavigate()

  useEffect(() => {
    let cancelado = false
    ;(async () => {
      try {
        const p = await ordersApi.obtenerPorCodigo(codigo)
        if (!cancelado) setPedido(p)
      } catch (err) {
        if (!cancelado) {
          setError(err instanceof ApiError ? err.message : 'No se pudo cargar el pedido.')
        }
      } finally {
        if (!cancelado) setCargando(false)
      }
    })()
    return () => {
      cancelado = true
    }
  }, [codigo])

  async function decidir(aprobado: boolean) {
    setProcesando(aprobado ? 'aprobado' : 'rechazado')
    setError(null)
    try {
      await ordersApi.confirmarPagoDemo(codigo, aprobado)
      navigate(`/carrito/gracias/${encodeURIComponent(codigo)}`)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'No se pudo procesar el pago.')
      setProcesando(null)
    }
  }

  if (cargando) {
    return <main style={{ padding: 'var(--space-6)' }}>Cargando pedido…</main>
  }

  if (!pedido) {
    return (
      <main style={{ maxWidth: 520, margin: '0 auto', padding: 'var(--space-6)' }}>
        <h1>No encontramos ese pedido</h1>
        <p>{error ?? `El código ${codigo} no existe.`}</p>
        <Link to="/catalogo" className="btn btn-primary" style={{ textDecoration: 'none' }}>
          Volver al catálogo
        </Link>
      </main>
    )
  }

  const unidades = pedido.items.reduce((sum, i) => sum + i.cantidad, 0)

  return (
    <main className="pay-page">
      <p className="pay-sandbox" role="note">
        Pasarela en modo de prueba: aquí no se cobra nada. En producción esta pantalla es el checkout de Wompi
        (tarjeta, PSE, Nequi o Bancolombia).
      </p>
      <div className="pay-grid">
        <section className="pay-summary" aria-labelledby="pay-resumen">
          <h1 id="pay-resumen" className="pay-title">Pagar pedido</h1>
          <p className="text-muted pay-code">Orden {pedido.codigo} · {unidades} {unidades === 1 ? 'mueble' : 'muebles'}</p>
          <ul className="pay-items">
            {pedido.items.map(item => (
              <li key={item.id}>
                <span>
                  {item.nombreProducto}
                  {item.varianteNombre && <span className="text-muted">, {item.varianteNombre}</span>}
                  {item.cantidad > 1 && <span className="text-muted"> × {item.cantidad}</span>}
                </span>
                <span className="pay-num">{monedaCOP.format(item.subtotal)}</span>
              </li>
            ))}
          </ul>
          <dl className="pay-meta">
            <dt>Cliente</dt><dd>{pedido.cliente}</dd>
            <dt>Despacho</dt><dd>{pedido.ciudad}, {pedido.envioCOP === 0 ? 'envío gratis' : monedaCOP.format(pedido.envioCOP)}</dd>
          </dl>
        </section>

        <section className="pay-panel" aria-label="Resultado del pago">
          <p className="pay-label">Total a pagar</p>
          <p className="pay-total">{monedaCOP.format(pedido.total)}</p>

          {error && (
            <p role="alert" className="pay-error">{error}</p>
          )}

          <button
            type="button"
            className="btn btn-primary pay-btn"
            disabled={procesando !== null}
            onClick={() => decidir(true)}
          >
            {procesando === 'aprobado' ? 'Aprobando…' : 'Aprobar pago'}
          </button>
          <button
            type="button"
            className="btn btn-secondary pay-btn"
            disabled={procesando !== null}
            onClick={() => decidir(false)}
          >
            {procesando === 'rechazado' ? 'Rechazando…' : 'Rechazar pago'}
          </button>
          <p className="text-muted pay-hint">Elige un resultado para ver cómo responde la tienda en cada caso.</p>
        </section>
      </div>
    </main>
  )
}
