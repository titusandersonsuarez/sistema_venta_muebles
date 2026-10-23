import { useEffect, useState } from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import * as ordersApi from '../../api/orders'
import * as productsApi from '../../api/products'
import { API_URL, ApiError } from '../../api/client'
import type { Order } from '../../types/order'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

const fechaLarga = new Intl.DateTimeFormat('es-CO', { day: 'numeric', month: 'long', year: 'numeric' })

/** Recorrido de un pedido pagado, en el orden en que lo vive el cliente. */
const ETAPAS = ['Pago confirmado', 'En taller', 'En ruta', 'Entregado'] as const

function absolutoImg(url: string): string {
  if (/^https?:/i.test(url)) return url
  const base = API_URL.replace(/\/api\/?$/, '')
  return `${base}${url.startsWith('/') ? '' : '/'}${url}`
}

export function GraciasPage() {
  const { codigo = '' } = useParams<{ codigo: string }>()
  const [searchParams] = useSearchParams()
  const [pedido, setPedido] = useState<Order | null>(null)
  const [fotos, setFotos] = useState<Record<number, string>>({})
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

  // Las fotos son un extra: si el catálogo no responde, la orden se ve igual.
  useEffect(() => {
    if (!pedido) return
    let cancelado = false
    productsApi
      .listarPublico({ tamano: 100 })
      .then(r => {
        if (cancelado) return
        const mapa: Record<number, string> = {}
        for (const p of r.items) if (p.imagenUrl) mapa[p.id] = absolutoImg(p.imagenUrl)
        setFotos(mapa)
      })
      .catch(() => {})
    return () => {
      cancelado = true
    }
  }, [pedido])

  if (cargando) {
    return <main className="ticket-page"><p className="text-muted">Cargando tu pedido…</p></main>
  }

  if (notFound || !pedido) {
    return (
      <main className="ticket-page">
        <h1>No encontramos ese pedido</h1>
        <p>El código {codigo} no existe. Revisa el enlace que te compartimos.</p>
        <Link to="/catalogo" className="btn btn-primary" style={{ textDecoration: 'none' }}>
          Ver el catálogo
        </Link>
      </main>
    )
  }

  const nombre = pedido.cliente.split(' ')[0]
  const { titulo, mensaje } = mensajeSegunEstado(pedido, nombre)
  const unidades = pedido.items.reduce((s, i) => s + i.cantidad, 0)
  const etapaActual = ETAPAS.indexOf(pedido.estado as (typeof ETAPAS)[number])
  const pagado = etapaActual >= 0

  return (
    <main className="ticket-page">
      <article className="ticket" aria-labelledby="ticket-titulo">
        <header className="ticket-head">
          <div>
            <span className="ticket-brand">Nogal</span>
            <span className="ticket-origin">Taller de Puente Aranda, Bogotá</span>
          </div>
          <dl className="ticket-ref">
            <dt>Orden</dt>
            <dd>{pedido.codigo}</dd>
            <dt>Fecha</dt>
            <dd>{fechaLarga.format(new Date(pedido.createdAt))}</dd>
          </dl>
        </header>

        <h1 id="ticket-titulo" className="ticket-title">{titulo}</h1>
        <p className="ticket-lead">{mensaje}</p>

        {pagado && (
          <ol className="ticket-track" aria-label="Estado del pedido">
            {ETAPAS.map((etapa, i) => (
              <li
                key={etapa}
                className={i < etapaActual ? 'is-done' : i === etapaActual ? 'is-current' : undefined}
                aria-current={i === etapaActual ? 'step' : undefined}
              >
                <span className="ticket-dot" aria-hidden="true" />
                {etapa}
              </li>
            ))}
          </ol>
        )}
        {!pagado && <p className={`ticket-status ${pedido.estado === 'Pago rechazado' ? 'is-alert' : ''}`}>{pedido.estado}</p>}

        <h2 className="ticket-section">{unidades === 1 ? 'Tu mueble' : `Tus ${unidades} muebles`}</h2>
        <ul className="ticket-items">
          {pedido.items.map(item => (
            <li key={item.id}>
              <span
                className="ticket-thumb"
                style={fotos[item.productId] ? { backgroundImage: `url("${fotos[item.productId]}")` } : undefined}
                aria-hidden="true"
              />
              <div>
                <strong>{item.nombreProducto}</strong>
                <span className="text-muted">
                  {item.varianteNombre ? `${item.varianteNombre}, ` : ''}
                  {item.cantidad} {item.cantidad === 1 ? 'unidad' : 'unidades'} de {monedaCOP.format(item.precioUnitario)}
                </span>
              </div>
              <span className="ticket-amount">{monedaCOP.format(item.subtotal)}</span>
            </li>
          ))}
        </ul>

        <dl className="ticket-totals">
          <div>
            <dt>Subtotal</dt>
            <dd>{monedaCOP.format(pedido.total - pedido.envioCOP)}</dd>
          </div>
          <div>
            <dt>Envío a {pedido.ciudad}</dt>
            <dd>{pedido.envioCOP === 0 ? 'Gratis' : monedaCOP.format(pedido.envioCOP)}</dd>
          </div>
          <div className="ticket-grand">
            <dt>{pagado ? 'Total pagado' : 'Total'}</dt>
            <dd>{monedaCOP.format(pedido.total)}</dd>
          </div>
        </dl>

        <footer className="ticket-foot">
          <p>
            <span className="text-muted">Para</span> {pedido.cliente}, {pedido.ciudad}
            {pedido.contacto && <> · {pedido.contacto}</>}
          </p>
          <p className="text-muted">10 años de garantía de fábrica. Guarda este código para cualquier consulta.</p>
        </footer>
      </article>

      <div className="ticket-actions">
        {pedido.estado === 'Pago rechazado' && (
          <Link
            to={`/carrito/pago-demo/${encodeURIComponent(pedido.codigo)}`}
            className="btn btn-primary"
            style={{ textDecoration: 'none' }}
          >
            Reintentar el pago
          </Link>
        )}
        <Link to="/catalogo" className={`btn ${pedido.estado === 'Pago rechazado' ? 'btn-secondary' : 'btn-primary'}`} style={{ textDecoration: 'none' }}>
          Seguir comprando
        </Link>
        <Link to="/" className="btn btn-ghost" style={{ textDecoration: 'none' }}>
          Volver al inicio
        </Link>
      </div>
    </main>
  )
}

function mensajeSegunEstado(pedido: Order, nombre: string): { titulo: string; mensaje: string } {
  const contacto = pedido.contacto ?? 'tu WhatsApp o correo'
  switch (pedido.estado) {
    case 'Pago confirmado':
    case 'En taller':
    case 'En ruta':
    case 'Entregado':
      return {
        titulo: `¡Pago confirmado, ${nombre}!`,
        mensaje: `Tu pedido ya está en la fila del taller. Te escribimos al ${contacto} cuando salga para ${pedido.ciudad}.`
      }
    case 'Pago rechazado':
      return {
        titulo: 'El pago no se pudo procesar',
        mensaje: `La pasarela rechazó el pago. Puedes intentarlo de nuevo o escribirnos al ${contacto} y coordinamos otra forma de pago.`
      }
    default:
      return {
        titulo: `Gracias, ${nombre}.`,
        mensaje: `Registramos tu pedido. Alguien del taller te contacta al ${contacto} para coordinar el pago y el despacho a ${pedido.ciudad}.`
      }
  }
}
