import { useEffect, useMemo, useState } from 'react'
import * as ordersApi from '../../api/orders'
import * as productsApi from '../../api/products'
import { ApiError } from '../../api/client'
import type { OrderListItem } from '../../types/order'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

const fechaCorta = new Intl.DateTimeFormat('es-CO', {
  day: '2-digit',
  month: 'short',
  year: 'numeric'
})

const TODOS = 'Todos'

/**
 * Estilo del .tag por estado. En taller y Pago confirmado son accent
 * porque son los estados en los que el pedido necesita acción del
 * equipo. Entregado es neutral (cerrado). Pendiente y rechazado son
 * outline (esperando algo externo).
 */
function tagClasePorEstado(estado: string): string {
  switch (estado) {
    case 'En taller':
    case 'Pago confirmado':
      return 'tag tag-accent'
    case 'Entregado':
      return 'tag tag-neutral'
    case 'En ruta':
    case 'Pago pendiente':
    case 'Pago rechazado':
    default:
      return 'tag tag-outline'
  }
}

const fechaHora = new Intl.DateTimeFormat('es-CO', {
  day: '2-digit',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit'
})

export function PedidosPage() {
  const [pedidos, setPedidos] = useState<OrderListItem[]>([])
  const [estados, setEstados] = useState<string[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [filtro, setFiltro] = useState<string>(TODOS)
  const [aviso, setAviso] = useState<string | null>(null)
  const [ocupadoId, setOcupadoId] = useState<number | null>(null)

  useEffect(() => {
    let cancelado = false
    ;(async () => {
      try {
        const opciones = await productsApi.obtenerOpciones()
        if (!cancelado) setEstados(opciones.estadosPedido ?? [])
      } catch {
        // opciones son un extra — si fallan, seguimos igual
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
        const page = await ordersApi.listar(1, 100, filtro === TODOS ? undefined : filtro)
        if (cancelado) return
        setPedidos(page.items)
        setError(null)
      } catch (err) {
        if (!cancelado) setError(mensajeDeError(err, 'No pudimos cargar los pedidos.'))
      } finally {
        if (!cancelado) setCargando(false)
      }
    })()
    return () => {
      cancelado = true
    }
  }, [filtro])

  function mostrarAviso(mensaje: string) {
    setAviso(mensaje)
    window.setTimeout(() => setAviso((actual) => (actual === mensaje ? null : actual)), 3500)
  }

  async function actualizarEstado(pedido: OrderListItem, nuevoEstado: string) {
    if (nuevoEstado === pedido.estado) return
    setOcupadoId(pedido.id)
    try {
      const actualizado = await ordersApi.cambiarEstado(pedido.id, nuevoEstado)
      setPedidos((prev) =>
        prev
          .map((p) => (p.id === pedido.id ? { ...p, estado: actualizado.estado } : p))
          .filter((p) => filtro === TODOS || p.estado === filtro)
      )
      mostrarAviso(`${pedido.codigo} → ${actualizado.estado}`)
    } catch (err) {
      setError(mensajeDeError(err, 'No pudimos cambiar el estado.'))
    } finally {
      setOcupadoId(null)
    }
  }

  const totalPeriodo = useMemo(
    () => pedidos.reduce((acc, p) => acc + p.total, 0),
    [pedidos]
  )

  const opcionesFiltro = useMemo(() => [TODOS, ...estados], [estados])

  return (
    <section>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'baseline',
          gap: 'var(--space-4)',
          flexWrap: 'wrap',
          marginBottom: 'var(--space-3)'
        }}
      >
        <div>
          <h3 style={{ fontWeight: 400, margin: 0 }}>Pedidos recientes</h3>
          <p className="text-muted" style={{ fontSize: 13, margin: 'var(--space-1) 0 0' }}>
            {cargando
              ? 'Cargando…'
              : `${pedidos.length} pedido${pedidos.length === 1 ? '' : 's'} · Total del listado ${monedaCOP.format(totalPeriodo)}`}
          </p>
        </div>
        {aviso && <span className="tag tag-accent">{aviso}</span>}
      </div>

      <div
        style={{
          display: 'flex',
          gap: 'var(--space-3)',
          alignItems: 'center',
          flexWrap: 'wrap',
          marginBottom: 'var(--space-3)'
        }}
      >
        <label htmlFor="filtro-estado" style={{ fontSize: 13 }}>Filtrar por estado:</label>
        <select
          id="filtro-estado"
          className="input"
          value={filtro}
          onChange={(e) => setFiltro(e.target.value)}
          style={{ maxWidth: 220 }}
        >
          {opcionesFiltro.map((e) => (
            <option key={e} value={e}>{e}</option>
          ))}
        </select>
      </div>

      {error && (
        <p
          role="alert"
          style={{
            fontSize: 12,
            color: 'var(--color-accent-700)',
            borderLeft: '1px solid var(--color-accent)',
            paddingLeft: 'var(--space-2)',
            margin: '0 0 var(--space-3)'
          }}
        >
          {error}
        </p>
      )}

      <div style={{ overflowX: 'auto' }}>
        <table className="table">
          <thead>
            <tr>
              <th>Pedido</th>
              <th>Cliente</th>
              <th>Ciudad</th>
              <th>Producto</th>
              <th style={{ textAlign: 'right' }}>Valor</th>
              <th>Estado</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {!cargando && pedidos.length === 0 && (
              <tr>
                <td colSpan={7} className="text-muted" style={{ textAlign: 'center', padding: 'var(--space-4)' }}>
                  {filtro === TODOS ? 'Todavía no hay pedidos.' : `Ningún pedido con estado "${filtro}".`}
                </td>
              </tr>
            )}

            {pedidos.map((p) => {
              const bloqueado = ocupadoId === p.id
              return (
                <tr key={p.id}>
                  <td style={{ fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' }}>
                    {p.codigo}
                    <div className="text-muted" style={{ fontSize: 11 }}>
                      {fechaCorta.format(new Date(p.createdAt))}
                    </div>
                    {p.pagoTransaccionId && (
                      <div className="text-muted" style={{ fontSize: 10, letterSpacing: '0.02em' }}>
                        {p.pagoProveedor} · {p.pagoTransaccionId}
                        {p.pagoActualizadoEn && ` · ${fechaHora.format(new Date(p.pagoActualizadoEn))}`}
                      </div>
                    )}
                  </td>
                  <td style={{ whiteSpace: 'nowrap' }}>{p.cliente}</td>
                  <td className="text-muted" style={{ whiteSpace: 'nowrap' }}>{p.ciudad}</td>
                  <td>{p.resumenProductos}</td>
                  <td
                    style={{
                      fontVariantNumeric: 'tabular-nums',
                      textAlign: 'right',
                      whiteSpace: 'nowrap'
                    }}
                  >
                    {monedaCOP.format(p.total)}
                  </td>
                  <td>
                    <span className={tagClasePorEstado(p.estado)}>{p.estado}</span>
                  </td>
                  <td style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
                    <select
                      className="input"
                      value={p.estado}
                      onChange={(e) => actualizarEstado(p, e.target.value)}
                      disabled={bloqueado || estados.length === 0}
                      style={{ fontSize: 12, padding: '4px 8px' }}
                      aria-label={`Cambiar estado de ${p.codigo}`}
                    >
                      {estados.map((e) => (
                        <option key={e} value={e}>{e}</option>
                      ))}
                    </select>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </section>
  )
}

function mensajeDeError(err: unknown, fallback: string): string {
  if (err instanceof ApiError) return err.message
  if (err instanceof Error) return err.message
  return fallback
}
