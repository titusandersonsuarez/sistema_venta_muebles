import { useEffect, useState } from 'react'
import * as salesApi from '../../api/sales'
import { ApiError } from '../../api/client'
import type { SalesDashboard, SalesGranularity } from '../../types/sales'

const currency = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 })
const categoryColors = ['var(--color-accent)', 'var(--color-accent-700)', 'var(--color-neutral-500)', 'var(--color-neutral-800)']

function formatDate(date: Date) {
  return date.toISOString().slice(0, 10)
}

function rangeForDays(days: number) {
  const to = new Date()
  to.setDate(to.getDate() + 1)
  const from = new Date(to)
  from.setDate(from.getDate() - days)
  return { from: formatDate(from), to: formatDate(to) }
}

function errorMessage(error: unknown) {
  if (error instanceof ApiError) return error.message
  return 'No pudimos cargar el resumen. Intenta de nuevo.'
}

export function ResumenPage() {
  const inicial = rangeForDays(30)
  const [from, setFrom] = useState(inicial.from)
  const [to, setTo] = useState(inicial.to)
  const [granularity, setGranularity] = useState<SalesGranularity>('day')
  const [dashboard, setDashboard] = useState<SalesDashboard | null>(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [bucketSeleccionado, setBucketSeleccionado] = useState<string | null>(null)

  useEffect(() => {
    let activo = true
    setCargando(true)
    setError('')
    setBucketSeleccionado(null)

    salesApi.obtenerDashboard(from, to, granularity)
      .then((resultado) => {
        if (activo) setDashboard(resultado)
      })
      .catch((err: unknown) => {
        if (activo) setError(errorMessage(err))
      })
      .finally(() => {
        if (activo) setCargando(false)
      })

    return () => {
      activo = false
    }
  }, [from, to, granularity])

  function aplicarAtajo(days: number) {
    const rango = rangeForDays(days)
    setFrom(rango.from)
    setTo(rango.to)
  }

  const maxVentas = Math.max(...(dashboard?.buckets.map((bucket) => bucket.ventas) ?? [0]), 1)
  const totalCategorias = dashboard?.categorias.reduce((total, categoria) => total + categoria.porcentaje, 0) ?? 0
  let acumulado = 0
  const anillo = dashboard?.categorias.length
    ? dashboard.categorias.map((categoria, index) => {
        const inicio = acumulado
        acumulado += categoria.porcentaje
        return `${categoryColors[index % categoryColors.length]} ${inicio}% ${acumulado}%`
      }).join(', ')
    : 'var(--color-neutral-200) 0 100%'

  return (
    <div className="sales-dashboard">
      <div className="sales-toolbar">
        <div>
          <p className="card-kicker">Visión general</p>
          <h1 className="sales-title">Resumen</h1>
          <p className="text-muted">Ventas confirmadas, excluyendo pedidos con pago pendiente.</p>
        </div>
        <div className="sales-shortcuts" aria-label="Rangos rápidos">
          <button className="btn btn-ghost" type="button" onClick={() => aplicarAtajo(1)}>Hoy</button>
          <button className="btn btn-ghost" type="button" onClick={() => aplicarAtajo(7)}>7 días</button>
          <button className="btn btn-ghost" type="button" onClick={() => aplicarAtajo(30)}>30 días</button>
          <button className="btn btn-ghost" type="button" onClick={() => aplicarAtajo(90)}>90 días</button>
        </div>
      </div>

      <div className="sales-filters">
        <label className="field">Desde<input className="input" type="date" value={from} onChange={(event) => setFrom(event.target.value)} /></label>
        <label className="field">Hasta<input className="input" type="date" value={to} onChange={(event) => setTo(event.target.value)} /></label>
        <div className="field">
          <span>Granularidad</span>
          <div className="seg">
            {(['day', 'week', 'month'] as SalesGranularity[]).map((value) => (
              <label className="seg-opt" key={value}>
                <input type="radio" name="granularity" checked={granularity === value} onChange={() => setGranularity(value)} />
                {value === 'day' ? 'Día' : value === 'week' ? 'Semana' : 'Mes'}
              </label>
            ))}
          </div>
        </div>
      </div>

      {error && <p className="sales-error" role="alert">{error}</p>}
      {cargando && <p className="text-muted">Cargando resumen...</p>}

      {dashboard && !cargando && (
        <>
          <div className="sales-kpis">
            <article className="card sales-kpi"><span className="card-kicker">Ventas</span><strong>{currency.format(dashboard.totales.ventas)}</strong><span className="card-meta">Periodo seleccionado</span></article>
            <article className="card sales-kpi"><span className="card-kicker">Pedidos</span><strong>{dashboard.totales.pedidos}</strong><span className="card-meta">Pedidos confirmados</span></article>
            <article className="card sales-kpi"><span className="card-kicker">Unidades</span><strong>{dashboard.totales.unidades}</strong><span className="card-meta">Muebles vendidos</span></article>
            <article className="card sales-kpi"><span className="card-kicker">Ticket promedio</span><strong>{currency.format(dashboard.totales.ticketPromedio)}</strong><span className="card-meta">Por pedido</span></article>
          </div>

          <div className="sales-main-grid">
            <section className="card sales-chart-card">
              <div className="sales-section-heading"><div><span className="card-kicker">Evolución</span><h2 className="card-title">Ventas en el tiempo</h2></div><span className="card-meta">{dashboard.buckets.length} periodos</span></div>
              <div className="sales-chart" aria-label="Ventas por periodo">
                {dashboard.buckets.map((bucket) => (
                  <button className={`sales-bar-wrap${bucketSeleccionado === bucket.label ? ' is-selected' : ''}`} type="button" key={bucket.label} onClick={() => setBucketSeleccionado(bucketSeleccionado === bucket.label ? null : bucket.label)} title={`${bucket.label} · ${currency.format(bucket.ventas)} · ${bucket.pedidos} pedidos`}>
                    <span className="sales-bar" style={{ height: `${Math.max((bucket.ventas / maxVentas) * 100, bucket.ventas ? 8 : 2)}%` }} />
                    <span className="sales-bar-label">{bucket.label}</span>
                  </button>
                ))}
              </div>
              {bucketSeleccionado && (() => {
                const seleccionado = dashboard.buckets.find((bucket) => bucket.label === bucketSeleccionado)
                return seleccionado ? <p className="sales-detail">{seleccionado.label}: {currency.format(seleccionado.ventas)} en {seleccionado.pedidos} pedidos.</p> : null
              })()}
            </section>

            <section className="card sales-category-card">
              <div className="sales-section-heading"><div><span className="card-kicker">Distribución</span><h2 className="card-title">Mezcla por categoría</h2></div><span className="card-meta">{totalCategorias.toFixed(0)}%</span></div>
              <div className="sales-category-content">
                <div className="sales-ring" style={{ background: `conic-gradient(${anillo})` }}><span>{currency.format(dashboard.totales.ventas)}</span></div>
                <div className="sales-legend">
                  {dashboard.categorias.map((categoria, index) => <div className="sales-legend-row" key={categoria.categoria}><i style={{ background: categoryColors[index % categoryColors.length] }} /> <span>{categoria.categoria}</span><strong>{categoria.porcentaje.toFixed(0)}%</strong></div>)}
                  {!dashboard.categorias.length && <span className="text-muted">Sin ventas en el periodo.</span>}
                </div>
              </div>
            </section>
          </div>

          <section className="card">
            <div className="sales-section-heading"><div><span className="card-kicker">Ranking</span><h2 className="card-title">Más vendidos del periodo</h2></div></div>
            {dashboard.masVendidos.length ? <div className="sales-products">{dashboard.masVendidos.map((producto, index) => <div className="sales-product-row" key={`${producto.productId}-${producto.nombre}`}><span className="sales-product-rank">{index + 1}</span><span>{producto.nombre}</span><span className="card-meta">{producto.unidades} und.</span><strong>{currency.format(producto.ventas)}</strong></div>)}</div> : <p className="text-muted">Sin productos vendidos en el periodo.</p>}
          </section>
        </>
      )}
    </div>
  )
}
