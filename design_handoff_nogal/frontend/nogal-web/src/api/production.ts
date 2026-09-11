import { apiFetch } from './client'
import type { InventoryItem, ProductionSummary } from '../types/production'

export function obtenerResumen() {
  return apiFetch<ProductionSummary>('/admin/production')
}

export function listarInventario() {
  return apiFetch<InventoryItem[]>('/admin/inventory')
}
