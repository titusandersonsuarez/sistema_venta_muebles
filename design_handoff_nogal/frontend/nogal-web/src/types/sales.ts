export type SalesGranularity = 'day' | 'week' | 'month'

export interface SalesTotals {
  ventas: number
  pedidos: number
  unidades: number
  ticketPromedio: number
}

export interface SalesBucket {
  label: string
  from: string
  to: string
  ventas: number
  pedidos: number
}

export interface SalesCategory {
  categoria: string
  ventas: number
  pedidos: number
  porcentaje: number
}

export interface TopProductSales {
  productId: number
  nombre: string
  unidades: number
  ventas: number
}

export interface SalesDashboard {
  from: string
  to: string
  granularity: SalesGranularity
  totales: SalesTotals
  buckets: SalesBucket[]
  categorias: SalesCategory[]
  masVendidos: TopProductSales[]
}