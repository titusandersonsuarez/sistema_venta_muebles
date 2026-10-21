export interface Review {
  id: number
  autor: string
  rating: number
  texto: string
  fechaCreacion: string
}

export interface AdminReview {
  id: number
  productId: number
  productoNombre: string
  productoSlug: string
  autor: string
  rating: number
  texto: string
  aprobado: boolean
  fechaCreacion: string
}

export interface CreateReviewInput {
  autor: string
  rating: number
  texto: string
}

export interface ReviewSubmitted {
  mensaje: string
}
