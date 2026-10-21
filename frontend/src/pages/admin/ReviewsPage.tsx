import { useEffect, useMemo, useState } from 'react'
import * as reviewsApi from '../../api/reviews'
import { ApiError } from '../../api/client'
import type { AdminReview } from '../../types/review'

const fechaCorta = new Intl.DateTimeFormat('es-CO', {
  day: '2-digit',
  month: 'short',
  year: 'numeric'
})

type Filtro = 'pendiente' | 'aprobado' | 'todas'

export function ReviewsPage() {
  const [reseñas, setReseñas] = useState<AdminReview[]>([])
  const [filtro, setFiltro] = useState<Filtro>('pendiente')
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [aviso, setAviso] = useState<string | null>(null)
  const [ocupadoId, setOcupadoId] = useState<number | null>(null)

  useEffect(() => {
    let cancelado = false
    setCargando(true)
    ;(async () => {
      try {
        const page = await reviewsApi.listarAdmin(filtro === 'todas' ? undefined : filtro)
        if (cancelado) return
        setReseñas(page.items)
        setError(null)
      } catch (err) {
        if (!cancelado) setError(mensajeDeError(err, 'No pudimos cargar las reseñas.'))
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

  async function aprobar(review: AdminReview) {
    setOcupadoId(review.id)
    try {
      await reviewsApi.aprobar(review.id)
      setReseñas((prev) =>
        prev
          .map((r) => (r.id === review.id ? { ...r, aprobado: true } : r))
          .filter((r) => filtro === 'todas' || (filtro === 'aprobado' ? r.aprobado : !r.aprobado))
      )
      mostrarAviso(`Reseña de ${review.autor} aprobada`)
    } catch (err) {
      setError(mensajeDeError(err, 'No pudimos aprobar la reseña.'))
    } finally {
      setOcupadoId(null)
    }
  }

  async function eliminar(review: AdminReview) {
    if (!window.confirm(`¿Eliminar la reseña de "${review.autor}"? Esta acción no se puede deshacer.`)) return
    setOcupadoId(review.id)
    try {
      await reviewsApi.eliminar(review.id)
      setReseñas((prev) => prev.filter((r) => r.id !== review.id))
      mostrarAviso('Reseña eliminada')
    } catch (err) {
      setError(mensajeDeError(err, 'No pudimos eliminar la reseña.'))
    } finally {
      setOcupadoId(null)
    }
  }

  const conteo = useMemo(() => reseñas.length, [reseñas])

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
          <h3 style={{ fontWeight: 400, margin: 0 }}>Bandeja de reseñas</h3>
          <p className="text-muted" style={{ fontSize: 13, margin: 'var(--space-1) 0 0' }}>
            {cargando
              ? 'Cargando…'
              : `${conteo} reseña${conteo === 1 ? '' : 's'} · filtro: ${etiquetaFiltro(filtro)}`}
          </p>
        </div>
        {aviso && <span className="tag tag-accent">{aviso}</span>}
      </div>

      <div className="seg" role="tablist" style={{ marginBottom: 'var(--space-3)' }}>
        {(['pendiente', 'aprobado', 'todas'] as Filtro[]).map((f) => (
          <button
            key={f}
            type="button"
            className={filtro === f ? 'is-active' : undefined}
            onClick={() => setFiltro(f)}
          >
            {etiquetaFiltro(f)}
          </button>
        ))}
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
              <th style={{ width: '18%' }}>Producto</th>
              <th style={{ width: '14%' }}>Autor</th>
              <th style={{ width: '8%', textAlign: 'center' }}>Rating</th>
              <th>Texto</th>
              <th style={{ width: '10%' }}>Fecha</th>
              <th style={{ width: '10%' }}>Estado</th>
              <th style={{ width: '14%', textAlign: 'right' }}></th>
            </tr>
          </thead>
          <tbody>
            {!cargando && reseñas.length === 0 && (
              <tr>
                <td colSpan={7} className="text-muted" style={{ textAlign: 'center', padding: 'var(--space-4)' }}>
                  {filtro === 'pendiente'
                    ? 'No hay reseñas por moderar. ¡Todo al día!'
                    : filtro === 'aprobado'
                      ? 'Todavía no se ha aprobado ninguna reseña.'
                      : 'No hay reseñas.'}
                </td>
              </tr>
            )}

            {reseñas.map((r) => {
              const bloqueado = ocupadoId === r.id
              return (
                <tr key={r.id}>
                  <td>
                    <div>{r.productoNombre}</div>
                    <div className="text-muted" style={{ fontSize: 11 }}>/{r.productoSlug}</div>
                  </td>
                  <td>{r.autor}</td>
                  <td style={{ textAlign: 'center', fontVariantNumeric: 'tabular-nums' }}>
                    {r.rating}/5
                  </td>
                  <td style={{ whiteSpace: 'pre-wrap' }}>{r.texto}</td>
                  <td className="text-muted" style={{ whiteSpace: 'nowrap', fontSize: 12 }}>
                    {fechaCorta.format(new Date(r.fechaCreacion))}
                  </td>
                  <td>
                    <span className={r.aprobado ? 'tag tag-neutral' : 'tag tag-outline'}>
                      {r.aprobado ? 'Aprobada' : 'Pendiente'}
                    </span>
                  </td>
                  <td style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
                    {!r.aprobado && (
                      <button
                        type="button"
                        className="btn btn-secondary"
                        style={{ fontSize: 12, padding: '4px 10px', marginRight: 'var(--space-2)' }}
                        disabled={bloqueado}
                        onClick={() => aprobar(r)}
                      >
                        Aprobar
                      </button>
                    )}
                    <button
                      type="button"
                      className="btn btn-ghost"
                      style={{ fontSize: 12, padding: '4px 10px' }}
                      disabled={bloqueado}
                      onClick={() => eliminar(r)}
                    >
                      Eliminar
                    </button>
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

function etiquetaFiltro(f: Filtro): string {
  switch (f) {
    case 'pendiente':
      return 'Pendientes'
    case 'aprobado':
      return 'Aprobadas'
    case 'todas':
      return 'Todas'
  }
}

function mensajeDeError(err: unknown, fallback: string): string {
  if (err instanceof ApiError) return err.message
  if (err instanceof Error) return err.message
  return fallback
}
