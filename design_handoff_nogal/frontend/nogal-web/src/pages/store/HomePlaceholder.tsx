import { FormEvent, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { API_URL, ApiError } from '../../api/client'
import { enviarContacto } from '../../api/communication'
import { listarPublico } from '../../api/products'
import { ArDialog } from '../../components/ArDialog'
import type { PublicProduct } from '../../types/product'

const moneda = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 })
const CATEGORIAS = ['Sofás', 'Sillas', 'Mesas', 'Camas']

export function HomePlaceholder() {
  const [productos, setProductos] = useState<PublicProduct[]>([])
  const [contacto, setContacto] = useState({ nombre: '', contacto: '', mensaje: '' })
  const [estadoContacto, setEstadoContacto] = useState('')
  const [arAbierto, setArAbierto] = useState(false)

  useEffect(() => { listarPublico({ pagina: 1, tamano: 12 }).then((r) => setProductos(r.items)).catch(() => undefined) }, [])

  const porCategoria = (categoria: string) => productos.filter((p) => p.categoria === categoria)
  const desde = (categoria: string) => {
    const precios = porCategoria(categoria).map((p) => p.precioCOP)
    return precios.length ? `desde ${moneda.format(Math.min(...precios))}` : 'Hecho a pedido'
  }

  async function enviarFormulario(event: FormEvent) {
    event.preventDefault(); setEstadoContacto('')
    try { await enviarContacto(contacto.nombre, contacto.contacto, contacto.mensaje); setContacto({ nombre: '', contacto: '', mensaje: '' }); setEstadoContacto('Gracias. Te responderemos muy pronto.') }
    catch (error) { setEstadoContacto(error instanceof ApiError ? error.message : 'No pudimos enviar tu mensaje.') }
  }

  return <main className="home-page">
    <section className="home-hero">
      <div><p className="home-kicker">Fábrica propia en Bogotá · Entrega en 5 días hábiles</p><h1>Somos la fábrica,<br />no el intermediario.</h1>
        <p className="home-lead">Salas, comedores, alcobas y sillas hechos en nuestro taller de Puente Aranda y despachados directo a tu casa. Sin distribuidor: por eso el precio es el que es.</p>
        <div className="home-actions"><Link to="/catalogo" className="btn btn-primary">Ver el catálogo</Link><a href="#contacto" className="btn btn-secondary">Verlo en tu casa</a></div>
        <div className="home-figures"><div><strong>4,8/5</strong><span>2.140 reseñas</span></div><div><strong>10 años</strong><span>de garantía de fábrica</span></div><div><strong>Gratis</strong><span>envío desde $ 500.000</span></div></div>
      </div><div className="plate home-photo"><span>sala Nogal · hecha en Puente Aranda</span></div>
    </section>

    <section className="home-section"><h2>Por espacio</h2><div className="home-category-grid">
      {CATEGORIAS.map((categoria) => <Link to="/catalogo" className="home-category" key={categoria}><div className="plate home-category-photo"><span>{categoria}</span></div><h3>{categoria}</h3><p>{desde(categoria)}</p></Link>)}
    </div></section>

    <section className="home-section"><div className="home-heading"><h2>Los más pedidos este mes</h2><Link to="/catalogo">Ver catálogo</Link></div><div className="home-product-grid">
      {productos.slice(0, 4).map((producto) => <Link to={`/producto/${producto.slug}`} className="home-product" key={producto.id}><div className="plate home-product-photo">{producto.imagenUrl ? <img src={imagenAbsoluta(producto.imagenUrl)} alt={producto.nombre} /> : <span>{producto.nombre}</span>}</div><h3>{producto.nombre}</h3><strong>{moneda.format(producto.precioCOP)}</strong><p>★★★★★ · despacho en 5 días</p></Link>)}
    </div></section>

    <section className="home-ar home-section" id="fabrica"><div><p className="home-kicker">Antes de decidir</p><h2>Ver en tu espacio</h2><p>Estamos preparando la experiencia para que pruebes el tamaño y acabado desde tu casa.</p><button className="btn btn-primary" type="button" onClick={() => setArAbierto(true)}>Probar con la cámara</button></div><div className="plate home-ar-photo"><span>tu sala · vista previa</span></div></section>

    <section className="home-section"><h2>Lo que dicen quienes ya tienen Nogal</h2><div className="home-review-grid"><blockquote>“La mesa llegó impecable y el acabado es aún más bonito que en fotos.”<footer>Camila · Mesa Sáchica</footer></blockquote><blockquote>“Nos ayudaron a ajustar las medidas para nuestra sala. Se nota que lo hacen ellos.”<footer>Andrés · Sofá Liena</footer></blockquote><blockquote>“Entrega puntual, armado cuidadoso y un mueble para muchos años.”<footer>María · Cama Arrayán</footer></blockquote></div></section>

    <section className="home-section"><div className="home-service-grid"><div><h3>Envío gratis</h3><p>Desde $ 500.000.</p></div><div><h3>Armado opcional</h3><p>$ 89.000, coordinado contigo.</p></div><div><h3>30 días</h3><p>Para pensarlo con calma.</p></div><div><h3>Paga a cuotas</h3><p>Tarjeta, PSE, Addi y Sistecrédito.</p></div></div></section>

    <section className="home-contact home-section" id="contacto"><form className="card" onSubmit={enviarFormulario}><p className="card-kicker">Contáctanos</p><h2>Hablemos de tu espacio</h2><label className="field">Nombre<input className="input" required value={contacto.nombre} onChange={(e) => setContacto({ ...contacto, nombre: e.target.value })} /></label><label className="field">Celular o correo<input className="input" required value={contacto.contacto} onChange={(e) => setContacto({ ...contacto, contacto: e.target.value })} /></label><label className="field">Mensaje<textarea className="input" required value={contacto.mensaje} onChange={(e) => setContacto({ ...contacto, mensaje: e.target.value })} /></label><button className="btn btn-primary" type="submit">Enviar mensaje</button>{estadoContacto && <p className="home-form-status">{estadoContacto}</p>}</form><div><div className="plate home-map"><span>Puente Aranda · Bogotá</span></div><table className="table home-address"><tbody><tr><td>Dirección</td><td>Cra. 56 #17-40, Bogotá</td></tr><tr><td>Horario</td><td>L–V 8:00–17:00 · Sáb 8:00–12:00</td></tr><tr><td>WhatsApp</td><td>300 000 0000</td></tr><tr><td>Correo</td><td>hola@nogal.com.co</td></tr></tbody></table></div></section>

    <footer className="home-footer"><div><h3>Nogal</h3><p>Fábrica de muebles en Puente Aranda, Bogotá.</p></div><div><h6>Comprar</h6><Link to="/catalogo">Catálogo</Link></div><div><h6>Ayuda</h6><a href="#contacto">Contacto</a></div><div><h6>Novedades</h6><input className="input" placeholder="Tu correo" /></div></footer>
    <ArDialog abierto={arAbierto} alCerrar={() => setArAbierto(false)} />
  </main>
}

function imagenAbsoluta(url: string): string {
  if (/^https?:/i.test(url)) return url
  return `${API_URL.replace(/\/api\/?$/, '')}${url.startsWith('/') ? '' : '/'}${url}`
}
