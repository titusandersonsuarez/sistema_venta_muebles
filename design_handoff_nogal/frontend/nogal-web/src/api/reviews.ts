import { apiFetch } from './client'
import type { AdminReview, CreateReviewInput, Review, ReviewSubmitted } from '../types/review'
import type { PagedResult } from '../types/product'

// Público
export function listarPorSlug(slug: string) {
  return apiFetch<Review[]>(`/products/${encodeURIComponent(slug)}/reviews`)
}

export function crear(slug: string, input: CreateReviewInput) {
  return apiFetch<ReviewSubmitted>(`/products/${encodeURIComponent(slug)}/reviews`, {
    method: 'POST',
    body: JSON.stringify(input)
  })
}

// Admin
export function listarAdmin(estado?: 'pendiente' | 'aprobado', pagina = 1, tamano = 50) {
  const params = new URLSearchParams({ pagina: String(pagina), tamano: String(tamano) })
  if (estado) params.set('estado', estado)
  return apiFetch<PagedResult<AdminReview>>(`/admin/reviews?${params}`)
}

export function aprobar(id: number) {
  return apiFetch<AdminReview>(`/admin/reviews/${id}/aprobar`, { method: 'POST' })
}

export function eliminar(id: number) {
  return apiFetch<void>(`/admin/reviews/${id}`, { method: 'DELETE' })
}
