export interface OrderItem {
  id: number
  productId: number
  nombreProducto: string
  cantidad: number
  precioUnitario: number
  subtotal: number
  productVariantId?: number | null
  varianteNombre?: string | null
  precioAjusteVariante: number
}

export interface Order {
  id: number
  codigo: string
  cliente: string
  ciudad: string
  contacto?: string | null
  total: number
  envioCOP: number
  estado: string
  createdAt: string
  pagoProveedor?: string | null
  pagoTransaccionId?: string | null
  pagoActualizadoEn?: string | null
  items: OrderItem[]
}

export interface PagoIntencion {
  checkoutUrl: string
  proveedor: string
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
  pagoProveedor?: string | null
  pagoTransaccionId?: string | null
  pagoActualizadoEn?: string | null
}

export interface CreateOrderItemInput {
  productId: number
  productVariantId?: number | null
  cantidad: number
}

export interface CreateOrderInput {
  cliente: string
  ciudad: string
  contacto: string
  items: CreateOrderItemInput[]
}

/**
 * Item persistido en el carrito del cliente. Se guarda todo lo necesario
 * para pintar la lista sin volver a pedir el producto — cuando el cliente
 * confirma, solo mandamos productId/variantId/cantidad y el servidor
 * recalcula precios.
 */
export interface CartItem {
  productId: number
  slug: string
  nombre: string
  imagenUrl?: string | null
  precioBase: number
  cantidad: number
  variante?: {
    id: number
    nombre: string
    tipo: string
    codigoColorHex?: string | null
    ajusteCOP: number
  } | null
}
