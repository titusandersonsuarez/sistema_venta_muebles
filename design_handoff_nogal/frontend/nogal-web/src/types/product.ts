export interface ProductImage {
  id: number
  url: string
  orden: number
}

export interface Product {
  id: number
  slug: string
  nombre: string
  categoria: string
  material: string
  precioCOP: number
  medidas?: string | null
  peso?: string | null
  armado?: string | null
  descripcion?: string | null
  estado: string
  imagenUrl?: string | null
  activo: boolean
  fechaCreacion: string
  fechaActualizacion: string
  imagenes: ProductImage[]
}

export interface PublicProduct {
  id: number
  slug: string
  nombre: string
  categoria: string
  material: string
  precioCOP: number
  medidas?: string | null
  peso?: string | null
  armado?: string | null
  descripcion?: string | null
  estado: string
  imagenUrl?: string | null
  imagenes: ProductImage[]
}

export interface PagedResult<T> {
  items: T[]
  total: number
  pagina: number
  tamano: number
}

export interface CreateProductPayload {
  nombre: string
  categoria: string
  material: string
  precioCOP: number
  medidas?: string
  peso?: string
  armado?: string
  descripcion?: string
  estado?: string
  imagenUrl?: string
}

export type UpdateProductPayload = CreateProductPayload

export interface CatalogOptions {
  categorias: string[]
  materiales: string[]
  estados: string[]
}
