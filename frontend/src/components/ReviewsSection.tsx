import { useEffect, useState } from 'react'
import * as reviewsApi from '../api/reviews'
import { ApiError } from '../api/client'
import type { Review } from '../types/review'

interface Props {
  slug: string
}

const fechaCorta = new Intl.DateTimeFormat('es-CO', {
  day: '2-digit',
  month: 'short',
  year: 'numeric'
})

export function ReviewsSection({ slug }: Props) {
  const [reseñas, setReseñas] = useState<Review[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [autor, setAutor] = useState('')
  const [rating, setRating] = useState(5)
  const [texto, setTexto] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [enviadoOk, setEnviadoOk] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false
    setCargando(true)
    ;(async () => {
      try {
        const data = await reviewsApi.listarPorSlug(slug)
        if (!cancelado) setReseñas(data)
      } catch {
        if (!cancelado) setReseñas([])
      } finally {
        if (!cancelado) setCargando(false)
      }
    })()
    return () => {
      cancelado = true
    }
  }, [slug])

  async function enviar(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setEnviadoOk(null)
    setEnviando(true)
    try {
      const res = await reviewsApi.crear(slug, { autor: autor.trim(), rating, texto: texto.trim() })
      setEnviadoOk(res.mensaje)
      setAutor('')
      setRating(5)
      setTexto('')
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('No pudimos enviar la reseña. Intenta más tarde.')
    } finally {
      setEnviando(false)
    }
  }

  const promedio =
    reseñas.length === 0 ? null : reseñas.reduce((acc, r) => acc + r.rating, 0) / reseñas.length

  return (
    <section style={{ marginTop: 'calc(var(--space-8) * 1.4)' }}>
      <hr className="hr" />
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'baseline',
          flexWrap: 'wrap',
          gap: 'var(--space-4)',
          marginBottom: 'var(--space-6)'
        }}
      >
        <h2 style={{ fontWeight: 400, margin: 0 }}>Lo que cuentan quienes ya lo tienen</h2>
        {promedio !== null && (
          <span className="text-muted" style={{ fontSize: 13 }}>
            <em>{promedio.toFixed(1)}</em>/5 · {reseñas.length} reseña{reseñas.length === 1 ? '' : 's'}
          </span>
        )}
      </div>

      {cargando ? (
        <p className="text-muted">Cargando reseñas…</p>
      ) : reseñas.length === 0 ? (
        <p className="text-muted" style={{ fontStyle: 'italic' }}>
          Todavía nadie deja una reseña de este mueble. Si ya lo tienes en casa, cuéntanos abajo cómo te ha ido.
        </p>
      ) : (
        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
            gap: 'var(--space-4)'
          }}
        >
          {reseñas.map((r) => (
            <article key={r.id} className="card" style={{ padding: 'var(--space-4)' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 'var(--space-2)' }}>
                <strong style={{ fontFamily: 'var(--font-heading)', fontWeight: 500 }}>{r.autor}</strong>
                <span style={{ letterSpacing: '0.05em' }}>{'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}</span>
              </div>
              <p style={{ margin: '0 0 var(--space-2)', lineHeight: 1.6 }}>{r.texto}</p>
              <span className="text-muted" style={{ fontSize: 11 }}>
                {fechaCorta.format(new Date(r.fechaCreacion))}
              </span>
            </article>
          ))}
        </div>
      )}

      <form
        onSubmit={enviar}
        className="card"
        style={{
          marginTop: 'var(--space-6)',
          padding: 'var(--space-6)',
          display: 'grid',
          gap: 'var(--space-3)'
        }}
      >
        <h4 style={{ fontWeight: 400, margin: 0 }}>Deja tu reseña</h4>
        <p className="text-muted" style={{ fontSize: 12, margin: 0 }}>
          Cada reseña la revisa nuestro equipo antes de publicarla (para que sean auténticas).
        </p>

        <div className="field">
          <label htmlFor="review-autor" style={{ fontSize: 12 }}>Nombre</label>
          <input
            id="review-autor"
            className="input"
            value={autor}
            onChange={(e) => setAutor(e.target.value)}
            placeholder="Tu nombre"
            maxLength={120}
            required
            disabled={enviando}
          />
        </div>

        <div className="field">
          <label htmlFor="review-rating" style={{ fontSize: 12 }}>Calificación</label>
          <div id="review-rating" style={{ display: 'flex', gap: 'var(--space-2)' }}>
            {[1, 2, 3, 4, 5].map((n) => (
              <button
                key={n}
                type="button"
                onClick={() => setRating(n)}
                aria-label={`${n} estrellas`}
                disabled={enviando}
                style={{
                  fontSize: 22,
                  lineHeight: 1,
                  padding: '4px 6px',
                  background: 'transparent',
                  border: 'none',
                  cursor: 'pointer',
                  color: n <= rating ? 'var(--color-accent-700)' : 'var(--color-divider)'
                }}
              >
                ★
              </button>
            ))}
          </div>
        </div>

        <div className="field">
          <label htmlFor="review-texto" style={{ fontSize: 12 }}>Tu experiencia</label>
          <textarea
            id="review-texto"
            className="input"
            value={texto}
            onChange={(e) => setTexto(e.target.value)}
            placeholder="Cuéntanos qué te pareció, cómo lo usas, si volverías a comprar…"
            rows={4}
            minLength={20}
            maxLength={2000}
            required
            disabled={enviando}
            style={{ resize: 'vertical' }}
          />
          <span className="text-muted" style={{ fontSize: 11 }}>
            {texto.trim().length}/2000 caracteres · mínimo 20
          </span>
        </div>

        {error && (
          <p
            role="alert"
            style={{
              fontSize: 12,
              color: 'var(--color-accent-700)',
              borderLeft: '1px solid var(--color-accent)',
              paddingLeft: 'var(--space-2)',
              margin: 0
            }}
          >
            {error}
          </p>
        )}
        {enviadoOk && (
          <p role="status" className="tag tag-accent" style={{ alignSelf: 'flex-start' }}>
            {enviadoOk}
          </p>
        )}

        <button type="submit" className="btn btn-primary" disabled={enviando} style={{ justifySelf: 'flex-start' }}>
          {enviando ? 'Enviando…' : 'Enviar reseña'}
        </button>
      </form>
    </section>
  )
}
