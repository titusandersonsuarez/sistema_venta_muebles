import { useEffect, useState } from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import * as ordersApi from '../../api/orders'
import { ApiError } from '../../api/client'
import type { Order } from '../../types/order'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

export function GraciasPage() {
  const { codigo = '' } = useParams<{ codigo: string }>()
  const [searchParams] = useSearchParams()
  const [pedido, setPedido] = useState<Order | null>(null)
  const [cargando, setCargando] = useState(true)
  const [notFound, setNotFound] = useState(false)

  // Wompi devuelve al usuario con ?id=<transactionId>&env=<sandbox|prod>.
  // Si viene el id, disparamos la verificación server-side para actualizar
  // el estado antes de mostrar la confirmación.
  const transactionId = searchParams.get('id')

  useEffect(() => {
    let cancelado = false
    setCargando(true)
    setNotFound(false)
    ;(async () => {
      try {
        const p = transactionId
          ? await ordersApi.verificarPago(codigo, transactionId)
          : await ordersApi.obtenerPorCodigo(codigo)
        if (!cancelado) setPedido(p)
      } catch (err) {
        if (!cancelado && err instanceof ApiError && err.status === 404) {
          setNotFound(true)
        }
      } finally {
        if (!cancelado) setCargando(false)
      }
    })()
    return () => {
      cancelado = true
    }
  }, [codigo, transactionId])

  if (cargando) {
    return <main style={{ padding: 'var(--space-6)' }}>Cargando pedido…</main>
  }

  if (notFound || !pedido) {
    return (
      <main style={{ maxWidth: 720, margin: '0 auto', padding: 'var(--space-6) max(var(--space-4), 4vw)' }}>
        <h1>No encontramos ese pedido</h1>
        <p>El código {codigo} no existe. Revisa el enlace que te compartieron.</p>
        <Link to="/catalogo" className="btn btn-primary" style={{ textDecoration: 'none' }}>
          Volver al catálogo
        </Link>
      </main>
    )
  }

  const nombre = pedido.cliente.split(' ')[0]
  const { titulo, mensaje, tagVariant } = mensajeSegunEstado(pedido, nombre)

  return (
    <main style={{ maxWidth: 720, margin: '0 auto', padding: 'var(--space-6) max(var(--space-4), 4vw)' }}>
      <p style={{ fontSize: 13, color: 'var(--color-text-muted)', marginBottom: 'var(--space-2)' }}>
        <Link to="/">Inicio</Link> · <Link to="/catalogo">Catálogo</Link> · Pedido {pedido.codigo}
      </p>
      <h1 style={{ margin: 0, marginBottom: 'var(--space-2)' }}>{titulo}</h1>
      <p style={{ marginBottom: 'var(--space-5)' }}>{mensaje}</p>

      <div className="card" style={{ padding: 'var(--space-5)', marginBottom: 'var(--space-5)' }}>
        <h2 style={{ marginTop: 0, marginBottom: 'var(--space-3)' }}>Resumen</h2>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
          {pedido.items.map(item => (
            <div
              key={item.id}
              style={{
                display: 'grid',
                gridTemplateColumns: '1fr auto',
                alignItems: 'baseline',
                gap: 'var(--space-3)',
                fontVariantNumeric: 'tabular-nums'
              }}
            >
              <div>
                <div>{item.nombreProducto}</div>
                <div style={{ fontSize: 12, color: 'var(--color-text-muted)' }}>
                  {item.cantidad} × {monedaCOP.format(item.precioUnitario)}
                  {item.varianteNombre && ` · ${item.varianteNombre}`}
                </div>
              </div>
              <div>{monedaCOP.format(item.subtotal)}</div>
            </div>
          ))}
        </div>
        <hr className="hr" style={{ margin: 'var(--space-4) 0' }} />
        <dl
          style={{
            display: 'grid',
            gridTemplateColumns: '1fr auto',
            rowGap: 'var(--space-2)',
            margin: 0,
            fontVariantNumeric: 'tabular-nums'
          }}
        >
          <dt>Subtotal</dt>
          <dd style={{ margin: 0, textAlign: 'right' }}>
            {monedaCOP.format(pedido.total - pedido.envioCOP)}
          </dd>
          <dt>Envío</dt>
          <dd style={{ margin: 0, textAlign: 'right' }}>
            {pedido.envioCOP === 0 ? 'Gratis' : monedaCOP.format(pedido.envioCOP)}
          </dd>
          <dt style={{ fontSize: 16, marginTop: 'var(--space-2)' }}>Total</dt>
          <dd style={{ margin: 0, textAlign: 'right', fontSize: 20, marginTop: 'var(--space-2)' }}>
            <strong>{monedaCOP.format(pedido.total)}</strong>
          </dd>
        </dl>
        <p style={{ fontSize: 12, color: 'var(--color-text-muted)', marginTop: 'var(--space-4)' }}>
          Estado: <span className={`tag ${tagVariant}`}>{pedido.estado}</span>
        </p>
      </div>

      <div style={{ display: 'flex', gap: 'var(--space-3)', flexWrap: 'wrap' }}>
        {pedido.estado === 'Pago rechazado' && (
          <Link
            to={`/carrito/pago-demo/${encodeURIComponent(pedido.codigo)}`}
            className="btn btn-primary"
            style={{ textDecoration: 'none' }}
          >
            Reintentar el pago
          </Link>
        )}
        <Link to="/catalogo" className="btn btn-secondary" style={{ textDecoration: 'none' }}>
          Seguir comprando
        </Link>
        <Link to="/" className="btn btn-ghost" style={{ textDecoration: 'none' }}>
          Volver al inicio
        </Link>
      </div>
    </main>
  )
}

function mensajeSegunEstado(pedido: Order, nombre: string): { titulo: string; mensaje: string; tagVariant: string } {
  const contacto = pedido.contacto ?? 'tu WhatsApp o correo'
  switch (pedido.estado) {
    case 'Pago confirmado':
      return {
        titulo: `¡Pago confirmado, ${nombre}!`,
        mensaje: `Recibimos tu pago del pedido ${pedido.codigo}. Ya lo mandamos al taller — te avisamos por ${contacto} cuando esté listo para despacho a ${pedido.ciudad}.`,
        tagVariant: 'tag-accent'
      }
    case 'Pago rechazado':
      return {
        titulo: 'El pago no se pudo procesar',
        mensaje: `La pasarela rechazó el intento de pago del pedido ${pedido.codigo}. Puedes reintentar aquí o escribirnos a ${contacto} y coordinamos otra forma de pago.`,
        tagVariant: 'tag-outline'
      }
    default:
      return {
        titulo: `Gracias, ${nombre}.`,
        mensaje: `Registramos tu pedido ${pedido.codigo}. Un miembro del taller te contacta a ${contacto} para coordinar el pago y el despacho a ${pedido.ciudad}.`,
        tagVariant: 'tag-outline'
      }
  }
}
