import { useEffect, useState } from 'react'
import { ApiError } from '../../api/client'
import { listarInventario, obtenerResumen } from '../../api/production'
import type { InventoryItem, ProductionSummary } from '../../types/production'

const tagPorEstado: Record<InventoryItem['estado'], string> = { Crítico: 'tag-accent', Bajo: 'tag-outline', Normal: 'tag-neutral' }
const formatoDias = new Intl.NumberFormat('es-CO', { minimumFractionDigits: 1, maximumFractionDigits: 1 })

export function ProduccionPage() {
  const [resumen, setResumen] = useState<ProductionSummary | null>(null)
  const [inventario, setInventario] = useState<InventoryItem[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let vigente = true
    Promise.all([obtenerResumen(), listarInventario()])
      .then(([datosProduccion, datosInventario]) => { if (vigente) { setResumen(datosProduccion); setInventario(datosInventario) } })
      .catch((reason: unknown) => { if (vigente) setError(reason instanceof ApiError ? reason.message : 'No fue posible cargar la producción.') })
      .finally(() => { if (vigente) setCargando(false) })
    return () => { vigente = false }
  }, [])

  if (cargando) return <p className="text-muted">Cargando órdenes e inventario…</p>
  if (error) return <p className="production-error">{error}</p>

  return <div className="production-dashboard">
    <section><div className="production-stage-grid">
      {resumen?.etapas.map((etapa) => <article className="card production-stage" key={etapa.nombre}>
        <p className="card-kicker">{etapa.nombre}</p><strong>{etapa.ordenes}</strong>
        <p className="card-meta">órdenes · {formatoDias.format(etapa.diasPromedio)} días prom.</p>
      </article>)}
    </div>{resumen && <p className="production-capacity">Capacidad del mes: {resumen.capacidadMensual} unidades. Comprometido: {resumen.comprometido}.</p>}</section>
    <section><h3 className="production-heading">Materiales por reponer</h3>
      {inventario.length === 0 ? <p className="text-muted">No hay materiales registrados.</p> : <div className="production-table-wrap"><table className="table"><tbody>
        {inventario.map((item) => <tr key={item.id}><td>{item.nombre}</td><td className="production-stock">{item.stock}</td><td className="production-status"><span className={`tag ${tagPorEstado[item.estado]}`}>{item.estado}</span></td></tr>)}
      </tbody></table></div>}
    </section>
  </div>
}
