import { apiFetch } from './client'
import type { Order, OrderListItem } from '../types/order'
import type { PagedResult } from '../types/product'

export function listar(pagina = 1, tamano = 50, estado?: string) {
  const params = new URLSearchParams({
    pagina: String(pagina),
    tamano: String(tamano)
  })
  if (estado) params.set('estado', estado)
  return apiFetch<PagedResult<OrderListItem>>(`/admin/orders?${params.toString()}`, { auth: true })
}

export function obtener(id: number) {
  return apiFetch<Order>(`/admin/orders/${id}`, { auth: true })
}

export function cambiarEstado(id: number, estado: string) {
  return apiFetch<Order>(`/admin/orders/${id}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ estado }),
    auth: true
  })
}
