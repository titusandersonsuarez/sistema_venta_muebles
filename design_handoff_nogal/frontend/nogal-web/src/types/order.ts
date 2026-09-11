export interface OrderItem {
  id: number
  productId: number
  nombreProducto: string
  cantidad: number
  precioUnitario: number
  subtotal: number
}

export interface Order {
  id: number
  codigo: string
  cliente: string
  ciudad: string
  total: number
  estado: string
  createdAt: string
  items: OrderItem[]
}

export interface OrderListItem {
  id: number
  codigo: string
  cliente: string
  ciudad: string
  total: number
  estado: string
  createdAt: string
  resumenProductos: string
}
