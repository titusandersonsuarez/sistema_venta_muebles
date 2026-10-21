import { apiFetch, apiUpload } from './client'
import type {
  CatalogOptions,
  CreateProductPayload,
  PagedResult,
  Product,
  PublicProduct,
  UpdateProductPayload
} from '../types/product'

export function listarAdmin(pagina = 1, tamano = 50, incluirInactivos = true) {
  const params = new URLSearchParams({
    pagina: String(pagina),
    tamano: String(tamano),
    incluirInactivos: String(incluirInactivos)
  })
  return apiFetch<PagedResult<Product>>(`/admin/products?${params.toString()}`)
}

export function crear(payload: CreateProductPayload) {
  return apiFetch<Product>('/admin/products', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function actualizar(id: number, payload: UpdateProductPayload) {
  return apiFetch<Product>(`/admin/products/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  })
}

export function eliminar(id: number) {
  return apiFetch<void>(`/admin/products/${id}`, {
    method: 'DELETE'
  })
}

export function restaurar(id: number) {
  return apiFetch<Product>(`/admin/products/${id}/restore`, {
    method: 'POST'
  })
}

export function subirImagen(id: number, archivo: File) {
  const form = new FormData()
  form.append('archivo', archivo)
  return apiUpload<Product>(`/admin/products/${id}/image`, form)
}

export function generarModelo3d(id: number) {
  return apiFetch<Product>(`/admin/products/${id}/3d-generation`, {
    method: 'POST'
  })
}

export function obtenerOpciones() {
  return apiFetch<CatalogOptions>('/catalog/options')
}

export function obtenerPorSlug(slug: string) {
  return apiFetch<PublicProduct>(`/products/${encodeURIComponent(slug)}`)
}

// Listado público con filtros (módulo #2).
export function listarPublico(params: {
  categoria?: string
  material?: string
  precioMax?: number
  pagina?: number
  tamano?: number
}) {
  const qs = new URLSearchParams()
  if (params.categoria) qs.set('categoria', params.categoria)
  if (params.material) qs.set('material', params.material)
  if (params.precioMax != null) qs.set('precioMax', String(params.precioMax))
  if (params.pagina != null) qs.set('pagina', String(params.pagina))
  if (params.tamano != null) qs.set('tamano', String(params.tamano))
  const query = qs.toString()
  return apiFetch<PagedResult<PublicProduct>>(`/products${query ? `?${query}` : ''}`)
}
