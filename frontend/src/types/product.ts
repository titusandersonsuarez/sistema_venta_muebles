export interface ProductImage {
  id: number
  url: string
  orden: number
}

export interface ProductVariant {
  id: number
  productId: number
  sku: string
  nombre: string
  tipo: string
  codigoColorHex?: string | null
  precioAjusteCOP: number
  fotoUrl?: string | null
  stock: number
  activo: boolean
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
  modelo3dUrl?: string | null
  modeloUsdzUrl?: string | null
  modelo3dEstado: string
  modelo3dError?: string | null
  modelo3dSolicitadoEn?: string | null
  activo: boolean
  fechaCreacion: string
  fechaActualizacion: string
  imagenes: ProductImage[]
  variantes?: ProductVariant[]
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
  modelo3dUrl?: string | null
  modeloUsdzUrl?: string | null
  modelo3dEstado: string
  modelo3dError?: string | null
  modelo3dSolicitadoEn?: string | null
  imagenes: ProductImage[]
  variantes?: ProductVariant[]
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
  modelo3dUrl?: string
  modeloUsdzUrl?: string
}

export type UpdateProductPayload = CreateProductPayload

export interface CatalogOptions {
  categorias: string[]
  materiales: string[]
  estados: string[]
  estadosPedido: string[]
}
