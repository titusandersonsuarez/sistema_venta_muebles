import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { API_URL, ApiError } from '../../api/client'
import { enviarContacto } from '../../api/communication'
import { listarPublico } from '../../api/products'
import { listarPorSlug } from '../../api/reviews'
import { ArDialog } from '../../components/ArDialog'
import type { PublicProduct } from '../../types/product'
import type { Review } from '../../types/review'

type ReseñaDestacada = Review & { producto: string }

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

const CATEGORIAS = ['Sofás', 'Sillas', 'Mesas', 'Camas'] as const
const ENVIO_DESDE = 500_000

const SERVICIOS = [
  {
    titulo: 'Envío gratis',
    detalle: `En compras desde ${monedaCOP.format(ENVIO_DESDE)}. Bogotá en 3 días, resto del país en 5 a 8 días hábiles.`
  },
  {
    titulo: 'Armado opcional',
    detalle: 'Lo armamos en tu casa por $ 89.000 y nos llevamos el empaque.'
  },
  {
    titulo: '30 días para pensarlo',
    detalle: 'Si no encaja, lo recogemos gratis y te devolvemos la plata.'
  },
  {
    titulo: 'Paga a cuotas',
    detalle: 'Hasta 12 cuotas con tarjeta, Addi o Sistecrédito.'
  }
]

export function HomePage() {
  const [productos, setProductos] = useState<PublicProduct[]>([])
  const [contacto, setContacto] = useState({ nombre: '', contacto: '', mensaje: '' })
  const [estadoContacto, setEstadoContacto] = useState('')
  const [enviandoContacto, setEnviandoContacto] = useState(false)
  const [arAbierto, setArAbierto] = useState(false)
  const [reseñas, setReseñas] = useState<ReseñaDestacada[]>([])

  useEffect(() => {
    listarPublico({ pagina: 1, tamano: 24 })
      .then(r => setProductos(r.items))
      .catch(() => undefined)
  }, [])

  const desdePorCategoria = useMemo(() => {
    const mapa = new Map<string, number>()
    for (const p of productos) {
      const actual = mapa.get(p.categoria)
      if (actual === undefined || p.precioCOP < actual) mapa.set(p.categoria, p.precioCOP)
    }
    return mapa
  }, [productos])

  // Primera foto real por categoría, para no mostrar recuadros vacíos.
  const fotoPorCategoria = useMemo(() => {
    const mapa = new Map<string, PublicProduct>()
    for (const p of productos) {
      if (p.imagenUrl && !mapa.has(p.categoria)) mapa.set(p.categoria, p)
    }
    return mapa
  }, [productos])

  const portada = fotoPorCategoria.get('Sofás') ?? productos.find(p => p.imagenUrl)

  const destacados = productos.slice(0, 4)

  // Reseñas reales (aprobadas en el panel) de los productos destacados.
  useEffect(() => {
    if (destacados.length === 0) return
    Promise.all(
      destacados.map(p =>
        listarPorSlug(p.slug)
          .then(rs => rs.map(r => ({ ...r, producto: p.nombre })))
          .catch(() => [] as ReseñaDestacada[])
      )
    ).then(listas =>
      setReseñas(
        listas
          .flat()
          .sort((x, y) => y.rating - x.rating)
          .slice(0, 3)
      )
    )
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [productos])

  async function enviarFormulario(event: FormEvent) {
    event.preventDefault()
    setEstadoContacto('')
    setEnviandoContacto(true)
    try {
      await enviarContacto(contacto.nombre, contacto.contacto, contacto.mensaje)
      setContacto({ nombre: '', contacto: '', mensaje: '' })
      setEstadoContacto('Gracias. Te responderemos el mismo día hábil.')
    } catch (error) {
      setEstadoContacto(error instanceof ApiError ? error.message : 'No pudimos enviar tu mensaje.')
    } finally {
      setEnviandoContacto(false)
    }
  }

  return (
    <main className="home-page">
      <section className="home-hero">
        <div>
          <p className="home-kicker">Fábrica propia en Bogotá · Entrega en 5 días hábiles</p>
          <h1>
            Somos la fábrica,<br />no el intermediario.
          </h1>
          <p className="home-lead">
            Salas, comedores, alcobas y sillas hechos en nuestro taller de Puente Aranda y despachados
            directo a tu casa. Sin showroom en centro comercial, sin distribuidor: por eso el precio es
            el que es.
          </p>
          <div className="home-actions">
            <Link to="/catalogo" className="btn btn-primary" style={{ fontSize: 15, padding: '12px 22px' }}>
              Ver el catálogo
            </Link>
            <button
              type="button"
              className="btn btn-secondary"
              style={{ fontSize: 15, padding: '12px 22px' }}
              onClick={() => setArAbierto(true)}
            >
              Verlo en tu casa
            </button>
          </div>
          <div className="home-figures">
            <div>
              <strong>{productos.length || '—'}</strong>
              <span>muebles en catálogo</span>
            </div>
            <div>
              <strong>10 años</strong>
              <span>de garantía de fábrica</span>
            </div>
            <div>
              <strong>Gratis</strong>
              <span>envío desde {monedaCOP.format(ENVIO_DESDE)}</span>
            </div>
          </div>
        </div>
        {portada?.imagenUrl ? (
          <Link to={`/producto/${portada.slug}`} className="plate home-photo has-img">
            <img src={imagenAbsoluta(portada.imagenUrl)} alt={portada.nombre} />
          </Link>
        ) : (
          <figure className="plate home-photo">
            <figcaption>Fotos del taller próximamente</figcaption>
          </figure>
        )}
      </section>

      <section className="home-section">
        <div className="home-heading">
          <h2>Por espacio</h2>
          <Link to="/catalogo">Ver las 4 categorías →</Link>
        </div>
        <div className="home-category-grid">
          {CATEGORIAS.map(categoria => {
            const desde = desdePorCategoria.get(categoria)
            return (
              <Link
                key={categoria}
                to={`/catalogo?categoria=${encodeURIComponent(categoria)}`}
                className="home-category"
              >
                <div className="plate home-category-photo">
                  {fotoPorCategoria.get(categoria)?.imagenUrl ? (
                    <img
                      src={imagenAbsoluta(fotoPorCategoria.get(categoria)!.imagenUrl!)}
                      alt={categoria}
                    />
                  ) : (
                    <span>{categoria.toLowerCase()}</span>
                  )}
                </div>
                <div
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'baseline',
                    gap: 'var(--space-2)',
                    marginTop: 'var(--space-2)'
                  }}
                >
                  <span style={{ fontFamily: 'var(--font-heading)', fontSize: 19 }}>{categoria}</span>
                  <span
                    className="text-muted"
                    style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}
                  >
                    {desde !== undefined ? `desde ${monedaCOP.format(desde)}` : 'Hecho a pedido'}
                  </span>
                </div>
              </Link>
            )
          })}
        </div>
      </section>

      <section className="home-section">
        <div className="home-heading">
          <h2>Los más pedidos este mes</h2>
          <Link to="/catalogo">Todo el catálogo →</Link>
        </div>
        <div className="home-product-grid">
          {destacados.map(producto => (
            <Link key={producto.id} to={`/producto/${producto.slug}`} className="home-product">
              <div className="plate home-product-photo">
                {producto.imagenUrl ? (
                  <img src={imagenAbsoluta(producto.imagenUrl)} alt={producto.nombre} />
                ) : (
                  <span>{producto.nombre}</span>
                )}
              </div>
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  gap: 'var(--space-2)',
                  alignItems: 'baseline',
                  marginTop: 'var(--space-2)'
                }}
              >
                <span style={{ fontFamily: 'var(--font-heading)', fontSize: 18 }}>{producto.nombre}</span>
                <strong style={{ fontVariantNumeric: 'tabular-nums', fontSize: 15, whiteSpace: 'nowrap' }}>
                  {monedaCOP.format(producto.precioCOP)}
                </strong>
              </div>
              <p className="text-muted" style={{ fontSize: 12, margin: 0 }}>
                {producto.material} · Despacho en 5 días
              </p>
            </Link>
          ))}
          {destacados.length === 0 && (
            <p className="text-muted" style={{ fontSize: 13 }}>
              Publicaremos pronto los muebles del mes.
            </p>
          )}
        </div>
      </section>

      <section className="home-ar home-section" id="fabrica">
        <div>
          <p className="home-kicker">Ver en tu espacio</p>
          <h2 style={{ fontSize: 34, marginTop: 'var(--space-2)' }}>
            Antes de comprar, ponlo en tu sala.
          </h2>
          <p style={{ maxWidth: '32em', textAlign: 'justify' }}>
            Apunta con el celular y el mueble aparece a tamaño real sobre tu piso. Revisas si cabe, si
            combina con la pared y si te convence — sin salir de casa ni pedir cita.
          </p>
          <button
            type="button"
            className="btn btn-primary"
            style={{ marginTop: 'var(--space-2)' }}
            onClick={() => setArAbierto(true)}
          >
            Probar con la cámara
          </button>
        </div>
        {portada?.imagenUrl ? (
          <div className="plate home-ar-photo has-img">
            <img src={imagenAbsoluta(portada.imagenUrl)} alt={`${portada.nombre} en una sala`} />
            <span className="home-ar-badge">AR · {portada.nombre}</span>
          </div>
        ) : (
          <div className="plate home-ar-photo">
            <span>Vista en realidad aumentada</span>
          </div>
        )}
      </section>

      {reseñas.length > 0 && (
        <section className="home-section">
          <h2>Lo que cuenta la gente</h2>
          <div className="home-review-grid" style={{ marginTop: 'var(--space-4)' }}>
            {reseñas.map(r => (
              <figure key={r.id}>
                <div
                  aria-label={`${r.rating} de 5`}
                  style={{ color: 'var(--color-accent-700)', fontSize: 13, letterSpacing: '0.1em' }}
                >
                  {'★'.repeat(r.rating)}
                  {'☆'.repeat(5 - r.rating)}
                </div>
                <blockquote>“{r.texto}”</blockquote>
                <footer>
                  {r.autor} · {r.producto}
                </footer>
              </figure>
            ))}
          </div>
        </section>
      )}

      <section className="home-section">
        <div className="home-service-grid">
          {SERVICIOS.map(s => (
            <div key={s.titulo}>
              <h3>{s.titulo}</h3>
              <p>{s.detalle}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="home-contact home-section" id="contacto">
        <div>
          <p className="home-kicker">Contáctanos</p>
          <h2 style={{ marginTop: 'var(--space-2)' }}>Escríbenos y te cotizamos</h2>
          <p className="text-muted" style={{ fontSize: 14, maxWidth: '30em' }}>
            Cuéntanos qué mueble buscas o qué medidas tienes. Respondemos el mismo día hábil.
          </p>
          <form onSubmit={enviarFormulario} style={{ maxWidth: '26em', marginTop: 'var(--space-4)' }}>
            <div className="field">
              <label htmlFor="home-nombre">Nombre</label>
              <input
                id="home-nombre"
                className="input"
                placeholder="Tu nombre"
                required
                value={contacto.nombre}
                onChange={e => setContacto({ ...contacto, nombre: e.target.value })}
                maxLength={120}
                autoComplete="name"
              />
            </div>
            <div className="field">
              <label htmlFor="home-contacto">Celular o correo</label>
              <input
                id="home-contacto"
                className="input"
                placeholder="300 000 0000"
                required
                value={contacto.contacto}
                onChange={e => setContacto({ ...contacto, contacto: e.target.value })}
                maxLength={120}
              />
            </div>
            <div className="field">
              <label htmlFor="home-mensaje">Mensaje</label>
              <textarea
                id="home-mensaje"
                className="input"
                placeholder="Quiero una mesa de 180 cm en roble…"
                required
                value={contacto.mensaje}
                onChange={e => setContacto({ ...contacto, mensaje: e.target.value })}
                rows={4}
              />
            </div>
            <button type="submit" className="btn btn-primary" disabled={enviandoContacto}>
              {enviandoContacto ? 'Enviando…' : 'Enviar mensaje'}
            </button>
            {estadoContacto && (
              <p className="home-form-status" role="status">
                {estadoContacto}
              </p>
            )}
          </form>
        </div>
        <div>
          <p className="home-kicker">Dónde estamos</p>
          <h2 style={{ marginTop: 'var(--space-2)' }}>Taller y punto de retiro</h2>
          <iframe
            className="plate home-map"
            style={{ marginBottom: 'var(--space-4)', width: '100%', border: 0 }}
            title="Mapa del taller en Puente Aranda, Bogotá"
            loading="lazy"
            src="https://www.openstreetmap.org/export/embed.html?bbox=-74.1195%2C4.6195%2C-74.1035%2C4.6315&layer=mapnik&marker=4.6255%2C-74.1115"
          />
          <table className="table home-address">
            <tbody>
              <tr>
                <td>Dirección</td>
                <td>Cra. 56 #17-40, Puente Aranda, Bogotá</td>
              </tr>
              <tr>
                <td>Horario</td>
                <td>Lun a vie 8:00 a. m. – 5:00 p. m. · Sáb 8:00 a. m. – 12:00 m.</td>
              </tr>
              <tr>
                <td>WhatsApp</td>
                <td style={{ fontVariantNumeric: 'tabular-nums' }}>300 000 0000</td>
              </tr>
              <tr>
                <td>Correo</td>
                <td>hola@nogal.com.co</td>
              </tr>
            </tbody>
          </table>
          <p className="text-muted" style={{ fontSize: 12, marginTop: 'var(--space-3)' }}>
            Puedes venir a ver los muebles terminados antes del despacho; avísanos por WhatsApp para
            tenerlos listos.
          </p>
        </div>
      </section>

      <footer className="home-footer">
        <div>
          <h3>Nogal</h3>
          <p className="text-muted" style={{ fontSize: 13 }}>
            Fábrica de muebles en Puente Aranda, Bogotá.
          </p>
          <p className="text-muted" style={{ fontSize: 12 }}>
            Cra. 56 #17-40 · 300 000 0000
          </p>
        </div>
        <div>
          <h6>Comprar</h6>
          <Link to="/catalogo">Catálogo</Link>
          <Link to="/catalogo?categoria=Sof%C3%A1s">Sofás</Link>
          <Link to="/catalogo?categoria=Mesas">Mesas</Link>
          <Link to="/carrito">Tu carrito</Link>
        </div>
        <div>
          <h6>Ayuda</h6>
          <a href="#contacto">Contáctanos</a>
          <a href="#fabrica">Ver en tu casa</a>
          <a href="https://wa.me/573000000000" target="_blank" rel="noreferrer">
            WhatsApp
          </a>
        </div>
        <div>
          <h6>Novedades</h6>
          <p className="text-muted" style={{ fontSize: 12, marginBottom: 'var(--space-2)' }}>
            Recibe una vez al mes nuestros nuevos muebles.
          </p>
          <form
            onSubmit={e => {
              e.preventDefault()
              // Placeholder: aún no hay servicio de newsletter.
            }}
            style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap' }}
          >
            <input
              className="input"
              type="email"
              placeholder="Tu correo"
              aria-label="Correo para novedades"
              style={{ minWidth: 0, flex: '1 1 140px' }}
            />
            <button type="submit" className="btn btn-secondary" style={{ fontSize: 13 }}>
              Suscribirme
            </button>
          </form>
        </div>
      </footer>

      <ArDialog abierto={arAbierto} alCerrar={() => setArAbierto(false)} />
    </main>
  )
}

function imagenAbsoluta(url: string): string {
  if (/^https?:/i.test(url)) return url
  return `${API_URL.replace(/\/api\/?$/, '')}${url.startsWith('/') ? '' : '/'}${url}`
}
