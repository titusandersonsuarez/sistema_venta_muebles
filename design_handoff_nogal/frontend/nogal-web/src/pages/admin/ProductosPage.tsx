import { useEffect, useMemo, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import * as productsApi from '../../api/products'
import { API_URL, ApiError } from '../../api/client'
import type { CatalogOptions, Product } from '../../types/product'

const monedaCOP = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0
})

interface BorradorNuevo {
  nombre: string
  categoria: string
  material: string
  precioCOP: string
  medidas: string
  descripcion: string
  archivo: File | null
}

const borradorVacio: BorradorNuevo = {
  nombre: '',
  categoria: '',
  material: '',
  precioCOP: '',
  medidas: '',
  descripcion: '',
  archivo: null
}

export function ProductosPage() {
  const [opciones, setOpciones] = useState<CatalogOptions | null>(null)
  const [productos, setProductos] = useState<Product[]>([])
  const [cargando, setCargando] = useState(true)
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null)
  const [guardado, setGuardado] = useState<string | null>(null)

  const [nuevo, setNuevo] = useState<BorradorNuevo>(borradorVacio)
  const [errorNuevo, setErrorNuevo] = useState<string | null>(null)
  const [publicando, setPublicando] = useState(false)

  const [ediciones, setEdiciones] = useState<Record<number, Product>>({})
  const [ocupadoId, setOcupadoId] = useState<number | null>(null)

  const inputFotoNuevoRef = useRef<HTMLInputElement | null>(null)

  const activos = useMemo(() => productos.filter((p) => p.activo), [productos])
  const eliminados = useMemo(() => productos.filter((p) => !p.activo), [productos])

  useEffect(() => {
    let cancelado = false
    ;(async () => {
      try {
        const [opts, page] = await Promise.all([
          productsApi.obtenerOpciones(),
          productsApi.listarAdmin(1, 100, true)
        ])
        if (cancelado) return
        setOpciones(opts)
        setProductos(page.items)
        setNuevo((prev) => ({
          ...prev,
          categoria: prev.categoria || opts.categorias[0] || '',
          material: prev.material || opts.materiales[0] || ''
        }))
      } catch (err) {
        if (cancelado) return
        setErrorGeneral(mensajeDeError(err, 'No pudimos cargar los productos.'))
      } finally {
        if (!cancelado) setCargando(false)
      }
    })()
    return () => {
      cancelado = true
    }
  }, [])

  function mostrarGuardado(mensaje: string) {
    setGuardado(mensaje)
    window.setTimeout(() => setGuardado((actual) => (actual === mensaje ? null : actual)), 3500)
  }

  async function publicar(evento: FormEvent) {
    evento.preventDefault()
    setErrorNuevo(null)

    const precio = Number(nuevo.precioCOP)
    if (!nuevo.nombre.trim()) {
      setErrorNuevo('El nombre del mueble es obligatorio.')
      return
    }
    if (!precio || precio <= 0) {
      setErrorNuevo('El precio debe ser mayor a cero.')
      return
    }

    setPublicando(true)
    try {
      const creado = await productsApi.crear({
        nombre: nuevo.nombre.trim(),
        categoria: nuevo.categoria,
        material: nuevo.material,
        precioCOP: precio,
        medidas: nuevo.medidas.trim() || undefined,
        descripcion: nuevo.descripcion.trim() || undefined
      })

      let finalCreado = creado
      if (nuevo.archivo) {
        finalCreado = await productsApi.subirImagen(creado.id, nuevo.archivo)
      }

      setProductos((prev) => [finalCreado, ...prev])
      setNuevo({
        ...borradorVacio,
        categoria: nuevo.categoria,
        material: nuevo.material
      })
      if (inputFotoNuevoRef.current) inputFotoNuevoRef.current.value = ''
      mostrarGuardado(`"${finalCreado.nombre}" publicado.`)
    } catch (err) {
      setErrorNuevo(mensajeDeError(err, 'No pudimos publicar el producto.'))
    } finally {
      setPublicando(false)
    }
  }

  function edicionActual(p: Product): Product {
    return ediciones[p.id] ?? p
  }

  function editarCampo<K extends keyof Product>(p: Product, campo: K, valor: Product[K]) {
    setEdiciones((prev) => ({
      ...prev,
      [p.id]: { ...edicionActual(p), [campo]: valor }
    }))
  }

  function hayCambios(p: Product): boolean {
    const editado = ediciones[p.id]
    if (!editado) return false
    return (
      editado.nombre !== p.nombre ||
      editado.categoria !== p.categoria ||
      editado.material !== p.material ||
      editado.precioCOP !== p.precioCOP ||
      (editado.descripcion ?? '') !== (p.descripcion ?? '') ||
      editado.estado !== p.estado ||
      (editado.medidas ?? '') !== (p.medidas ?? '')
    )
  }

  async function guardar(p: Product) {
    const actual = edicionActual(p)
    setOcupadoId(p.id)
    try {
      const actualizado = await productsApi.actualizar(p.id, {
        nombre: actual.nombre,
        categoria: actual.categoria,
        material: actual.material,
        precioCOP: Number(actual.precioCOP),
        medidas: actual.medidas ?? undefined,
        descripcion: actual.descripcion ?? undefined,
        estado: actual.estado
      })
      setProductos((prev) => prev.map((x) => (x.id === p.id ? actualizado : x)))
      setEdiciones((prev) => {
        const { [p.id]: _drop, ...resto } = prev
        void _drop
        return resto
      })
      mostrarGuardado(`"${actualizado.nombre}" actualizado.`)
    } catch (err) {
      setErrorGeneral(mensajeDeError(err, 'No pudimos guardar los cambios.'))
    } finally {
      setOcupadoId(null)
    }
  }

  function descartar(p: Product) {
    setEdiciones((prev) => {
      const { [p.id]: _drop, ...resto } = prev
      void _drop
      return resto
    })
  }

  async function eliminar(p: Product) {
    setOcupadoId(p.id)
    try {
      await productsApi.eliminar(p.id)
      setProductos((prev) => prev.map((x) => (x.id === p.id ? { ...x, activo: false } : x)))
      descartar(p)
      mostrarGuardado(`"${p.nombre}" eliminado.`)
    } catch (err) {
      setErrorGeneral(mensajeDeError(err, 'No pudimos eliminar el producto.'))
    } finally {
      setOcupadoId(null)
    }
  }

  async function devolver(p: Product) {
    setOcupadoId(p.id)
    try {
      const restaurado = await productsApi.restaurar(p.id)
      setProductos((prev) => prev.map((x) => (x.id === p.id ? restaurado : x)))
      mostrarGuardado(`"${restaurado.nombre}" restaurado.`)
    } catch (err) {
      setErrorGeneral(mensajeDeError(err, 'No pudimos restaurar el producto.'))
    } finally {
      setOcupadoId(null)
    }
  }

  async function subirFoto(p: Product, evento: ChangeEvent<HTMLInputElement>) {
    const archivo = evento.target.files?.[0]
    if (!archivo) return
    setOcupadoId(p.id)
    try {
      const actualizado = await productsApi.subirImagen(p.id, archivo)
      setProductos((prev) => prev.map((x) => (x.id === p.id ? actualizado : x)))
      mostrarGuardado(`Foto de "${p.nombre}" actualizada.`)
    } catch (err) {
      setErrorGeneral(mensajeDeError(err, 'No pudimos subir la imagen.'))
    } finally {
      evento.target.value = ''
      setOcupadoId(null)
    }
  }

  if (cargando) {
    return (
      <section>
        <p className="text-muted">Cargando productos…</p>
      </section>
    )
  }

  if (errorGeneral && productos.length === 0) {
    return (
      <section>
        <p role="alert" style={{ color: 'var(--color-accent-700)' }}>{errorGeneral}</p>
      </section>
    )
  }

  const categorias = opciones?.categorias ?? []
  const materiales = opciones?.materiales ?? []
  const estados = opciones?.estados ?? []

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
          <h3 style={{ fontWeight: 400, margin: 0 }}>Productos, fotos y precios</h3>
          <p className="text-muted" style={{ fontSize: 13, margin: 'var(--space-1) 0 0' }}>
            Sube la foto real de cada mueble y ajusta el precio: la tienda se actualiza de inmediato.
          </p>
        </div>
        {guardado && <span className="tag tag-accent">{guardado}</span>}
      </div>

      {errorGeneral && (
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
          {errorGeneral}
        </p>
      )}

      <form
        onSubmit={publicar}
        style={{
          border: '1px solid var(--color-divider)',
          borderRadius: 'var(--radius-md)',
          padding: 'var(--space-4)',
          marginBottom: 'var(--space-4)',
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit,minmax(190px,1fr))',
          gap: 'var(--space-3)',
          alignItems: 'end'
        }}
      >
        <div style={{ gridColumn: '1 / -1' }}>
          <h4 style={{ fontWeight: 400, margin: 0 }}>Subir un producto nuevo</h4>
        </div>

        <div className="field">
          <label htmlFor="pn-nombre">Nombre</label>
          <input
            id="pn-nombre"
            className="input"
            value={nuevo.nombre}
            onChange={(e) => setNuevo({ ...nuevo, nombre: e.target.value })}
            placeholder="Mesa auxiliar Sáchica"
          />
        </div>

        <div className="field">
          <label htmlFor="pn-categoria">Categoría</label>
          <select
            id="pn-categoria"
            className="input"
            value={nuevo.categoria}
            onChange={(e) => setNuevo({ ...nuevo, categoria: e.target.value })}
          >
            {categorias.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </select>
        </div>

        <div className="field">
          <label htmlFor="pn-material">Material</label>
          <select
            id="pn-material"
            className="input"
            value={nuevo.material}
            onChange={(e) => setNuevo({ ...nuevo, material: e.target.value })}
          >
            {materiales.map((m) => (
              <option key={m} value={m}>{m}</option>
            ))}
          </select>
        </div>

        <div className="field">
          <label htmlFor="pn-precio">Precio (COP)</label>
          <input
            id="pn-precio"
            className="input"
            type="number"
            step={10000}
            min={0}
            value={nuevo.precioCOP}
            onChange={(e) => setNuevo({ ...nuevo, precioCOP: e.target.value })}
            placeholder="490000"
            style={{ fontVariantNumeric: 'tabular-nums' }}
          />
        </div>

        <div className="field">
          <label htmlFor="pn-medidas">Medidas</label>
          <input
            id="pn-medidas"
            className="input"
            value={nuevo.medidas}
            onChange={(e) => setNuevo({ ...nuevo, medidas: e.target.value })}
            placeholder="60 × 40 × 55 cm"
          />
        </div>

        <div className="field" style={{ gridColumn: '1 / -1' }}>
          <label htmlFor="pn-desc">Descripción</label>
          <textarea
            id="pn-desc"
            className="input"
            value={nuevo.descripcion}
            onChange={(e) => setNuevo({ ...nuevo, descripcion: e.target.value })}
            placeholder="Para qué sirve, en qué madera va, qué trae incluido…"
            rows={2}
          />
        </div>

        <div
          style={{
            display: 'flex',
            gap: 'var(--space-3)',
            alignItems: 'center',
            flexWrap: 'wrap',
            gridColumn: '1 / -1'
          }}
        >
          <label style={{ display: 'inline-flex', alignItems: 'center', gap: 'var(--space-2)', cursor: 'pointer' }}>
            <span className="btn btn-secondary" style={{ fontSize: 13 }}>Elegir foto</span>
            <input
              ref={inputFotoNuevoRef}
              type="file"
              accept="image/*"
              onChange={(e) => setNuevo({ ...nuevo, archivo: e.target.files?.[0] ?? null })}
              style={{ display: 'none' }}
            />
            <span className="text-muted" style={{ fontSize: 12 }}>
              {nuevo.archivo ? nuevo.archivo.name : 'Sin foto todavía'}
            </span>
          </label>

          <button className="btn btn-primary" type="submit" disabled={publicando}>
            {publicando ? 'Publicando…' : 'Publicar producto'}
          </button>

          {errorNuevo && (
            <span
              role="alert"
              style={{
                fontSize: 12,
                color: 'var(--color-accent-700)',
                borderLeft: '1px solid var(--color-accent)',
                paddingLeft: 'var(--space-2)'
              }}
            >
              {errorNuevo}
            </span>
          )}
        </div>
      </form>

      <div style={{ overflowX: 'auto' }}>
        <table className="table">
          <thead>
            <tr>
              <th>Foto</th>
              <th>Mueble</th>
              <th>Categoría</th>
              <th>Precio (COP)</th>
              <th>Descripción</th>
              <th>Estado</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {activos.length === 0 && (
              <tr>
                <td colSpan={7} className="text-muted" style={{ textAlign: 'center', padding: 'var(--space-4)' }}>
                  Todavía no hay productos publicados. Sube el primero con el formulario de arriba.
                </td>
              </tr>
            )}

            {activos.map((p) => {
              const editando = edicionActual(p)
              const bg = editando.imagenUrl
                ? `center/cover url("${urlAbsoluta(editando.imagenUrl)}")`
                : 'var(--color-surface)'
              const modificado = hayCambios(p)
              const bloqueado = ocupadoId === p.id

              return (
                <tr key={p.id}>
                  <td>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', cursor: 'pointer' }}>
                      <span
                        className="plate"
                        style={{
                          width: 52,
                          height: 40,
                          flex: 'none',
                          borderRadius: 'var(--radius-sm)',
                          display: 'block',
                          background: bg
                        }}
                      />
                      <span className="btn btn-secondary" style={{ fontSize: 12, padding: '5px 10px' }}>
                        Subir
                      </span>
                      <input
                        type="file"
                        accept="image/*"
                        onChange={(e) => subirFoto(p, e)}
                        style={{ display: 'none' }}
                        disabled={bloqueado}
                      />
                    </label>
                  </td>
                  <td>
                    <input
                      className="input"
                      value={editando.nombre}
                      onChange={(e) => editarCampo(p, 'nombre', e.target.value)}
                      style={{ width: 190 }}
                    />
                  </td>
                  <td>
                    <select
                      className="input"
                      value={editando.categoria}
                      onChange={(e) => editarCampo(p, 'categoria', e.target.value)}
                    >
                      {categorias.map((c) => (
                        <option key={c} value={c}>{c}</option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <input
                      className="input"
                      type="number"
                      step={10000}
                      min={0}
                      value={editando.precioCOP}
                      onChange={(e) => editarCampo(p, 'precioCOP', Number(e.target.value))}
                      style={{ width: 140, fontVariantNumeric: 'tabular-nums' }}
                    />
                    <div className="text-muted" style={{ fontSize: 11, marginTop: 4, fontVariantNumeric: 'tabular-nums' }}>
                      {monedaCOP.format(editando.precioCOP || 0)}
                    </div>
                  </td>
                  <td>
                    <textarea
                      className="input"
                      value={editando.descripcion ?? ''}
                      onChange={(e) => editarCampo(p, 'descripcion', e.target.value)}
                      style={{ width: 300, minHeight: 72, fontSize: 13 }}
                    />
                  </td>
                  <td>
                    <select
                      className="input"
                      value={editando.estado}
                      onChange={(e) => editarCampo(p, 'estado', e.target.value)}
                    >
                      {estados.map((s) => (
                        <option key={s} value={s}>{s === 'EnProceso' ? 'En proceso' : s}</option>
                      ))}
                    </select>
                  </td>
                  <td style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
                    {modificado && (
                      <>
                        <button
                          className="btn btn-ghost"
                          onClick={() => guardar(p)}
                          style={{ fontSize: 12 }}
                          disabled={bloqueado}
                        >
                          Guardar
                        </button>
                        <button
                          className="btn btn-ghost"
                          onClick={() => descartar(p)}
                          style={{ fontSize: 12 }}
                          disabled={bloqueado}
                        >
                          Restaurar
                        </button>
                      </>
                    )}
                    <button
                      className="btn btn-ghost"
                      onClick={() => eliminar(p)}
                      style={{ fontSize: 12 }}
                      disabled={bloqueado}
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

      {eliminados.length > 0 && (
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 'var(--space-3)',
            flexWrap: 'wrap',
            marginTop: 'var(--space-3)',
            borderTop: '1px solid var(--color-divider)',
            paddingTop: 'var(--space-3)'
          }}
        >
          <span className="text-muted" style={{ fontSize: 12 }}>Eliminados:</span>
          {eliminados.map((p) => (
            <button
              key={p.id}
              className="tag tag-outline"
              onClick={() => devolver(p)}
              disabled={ocupadoId === p.id}
              style={{
                cursor: 'pointer',
                background: 'transparent',
                fontFamily: 'var(--font-body)'
              }}
            >
              {p.nombre} · devolver
            </button>
          ))}
        </div>
      )}
    </section>
  )
}

function urlAbsoluta(url: string): string {
  if (/^https?:/i.test(url)) return url
  // API_URL termina en /api → sacamos ese sufijo para construir /uploads/...
  const base = API_URL.replace(/\/api\/?$/, '')
  return `${base}${url.startsWith('/') ? '' : '/'}${url}`
}

function mensajeDeError(err: unknown, fallback: string): string {
  if (err instanceof ApiError) return err.message
  if (err instanceof Error) return err.message
  return fallback
}
