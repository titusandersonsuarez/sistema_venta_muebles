import { apiFetch } from './client'
import type { CreateOrderInput, Order, OrderListItem, PagoIntencion } from '../types/order'
import type { PagedResult } from '../types/product'

export function crear(dto: CreateOrderInput) {
  return apiFetch<Order>('/orders', {
    method: 'POST',
    body: JSON.stringify(dto)
  })
}

export function obtenerPorCodigo(codigo: string) {
  return apiFetch<Order>(`/orders/${encodeURIComponent(codigo)}`)
}

export function iniciarPago(codigo: string) {
  return apiFetch<PagoIntencion>(`/orders/${encodeURIComponent(codigo)}/pago`, {
    method: 'POST'
  })
}

export function confirmarPagoDemo(codigo: string, aprobado: boolean) {
  return apiFetch<Order>(`/orders/${encodeURIComponent(codigo)}/pago/demo`, {
    method: 'POST',
    body: JSON.stringify({ aprobado })
  })
}

export function verificarPago(codigo: string, transactionId: string) {
  const params = new URLSearchParams({ transactionId })
  return apiFetch<Order>(`/orders/${encodeURIComponent(codigo)}/pago/verificar?${params}`, {
    method: 'POST'
  })
}

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
