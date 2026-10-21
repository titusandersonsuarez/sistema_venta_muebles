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

  return (
    <main
      style={{
        display: 'grid',
        placeItems: 'center',
        padding: 'var(--space-6) max(var(--space-4), 4vw)',
        minHeight: 'calc(100vh - 120px)'
      }}
    >
      <div className="card" style={{ maxWidth: 520, width: '100%', padding: 'var(--space-6)' }}>
        <p
          className="text-muted"
          style={{
            fontSize: 11,
            letterSpacing: '0.14em',
            textTransform: 'uppercase',
            margin: 0,
            marginBottom: 'var(--space-2)'
          }}
        >
          Pasarela · Simulación
        </p>
        <h1 style={{ margin: 0, fontSize: 28 }}>Pagar pedido {pedido.codigo}</h1>
        <p className="text-muted" style={{ fontSize: 13, marginTop: 'var(--space-2)' }}>
          Estás en una pantalla de prueba. En producción esto lo maneja Wompi (tarjeta, PSE, Nequi,
          Bancolombia). Elige un resultado para simular el retorno.
        </p>

        <dl
          style={{
            display: 'grid',
            gridTemplateColumns: '1fr auto',
            rowGap: 'var(--space-2)',
            marginTop: 'var(--space-5)',
            fontVariantNumeric: 'tabular-nums'
          }}
        >
          <dt>Cliente</dt>
          <dd style={{ margin: 0, textAlign: 'right' }}>{pedido.cliente}</dd>
          <dt>Ciudad</dt>
          <dd style={{ margin: 0, textAlign: 'right' }}>{pedido.ciudad}</dd>
          <dt>Ítems</dt>
          <dd style={{ margin: 0, textAlign: 'right' }}>
            {pedido.items.reduce((sum, i) => sum + i.cantidad, 0)}
          </dd>
          <dt style={{ fontSize: 16, marginTop: 'var(--space-2)' }}>Total a pagar</dt>
          <dd
            style={{ margin: 0, textAlign: 'right', fontSize: 22, marginTop: 'var(--space-2)' }}
          >
            <strong>{monedaCOP.format(pedido.total)}</strong>
          </dd>
        </dl>

        {error && (
          <p
            role="alert"
            style={{
              color: 'var(--color-accent-700)',
              borderLeft: '2px solid var(--color-accent)',
              paddingLeft: 'var(--space-2)',
              marginTop: 'var(--space-4)',
              fontSize: 13
            }}
          >
            {error}
          </p>
        )}

        <div style={{ display: 'flex', gap: 'var(--space-3)', marginTop: 'var(--space-6)', flexWrap: 'wrap' }}>
          <button
            type="button"
            className="btn btn-primary"
            style={{ flex: '1 1 180px' }}
            disabled={procesando !== null}
            onClick={() => decidir(true)}
          >
            {procesando === 'aprobado' ? 'Aprobando…' : 'Aprobar pago'}
          </button>
          <button
            type="button"
            className="btn btn-secondary"
            style={{ flex: '1 1 180px' }}
            disabled={procesando !== null}
            onClick={() => decidir(false)}
          >
            {procesando === 'rechazado' ? 'Rechazando…' : 'Rechazar pago'}
          </button>
        </div>
      </div>
    </main>
  )
}
