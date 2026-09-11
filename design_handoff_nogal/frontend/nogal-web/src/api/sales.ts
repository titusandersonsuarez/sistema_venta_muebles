import { apiFetch } from './client'
import type { SalesDashboard, SalesGranularity } from '../types/sales'

export function obtenerDashboard(from: string, to: string, granularity: SalesGranularity) {
  const params = new URLSearchParams({ from, to, granularity })
  return apiFetch<SalesDashboard>(`/admin/sales?${params.toString()}`)
}