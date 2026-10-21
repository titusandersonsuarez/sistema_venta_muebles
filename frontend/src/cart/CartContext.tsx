import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import type { CartItem } from '../types/order'

const STORAGE_KEY = 'nogal_carrito'

interface CartContextValue {
  items: CartItem[]
  cantidad: number
  subtotalCOP: number
  agregar: (item: CartItem) => void
  quitar: (productId: number, variantId: number | null | undefined) => void
  setCantidad: (productId: number, variantId: number | null | undefined, cantidad: number) => void
  vaciar: () => void
}

const CartContext = createContext<CartContextValue | undefined>(undefined)

function claveItem(productId: number, variantId: number | null | undefined) {
  return `${productId}::${variantId ?? 'base'}`
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<CartItem[]>(() => {
    if (typeof window === 'undefined') return []
    try {
      const raw = window.localStorage.getItem(STORAGE_KEY)
      return raw ? (JSON.parse(raw) as CartItem[]) : []
    } catch {
      return []
    }
  })

  useEffect(() => {
    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(items))
    } catch {
      // Storage lleno o deshabilitado: el carrito seguirá en memoria.
    }
  }, [items])

  const agregar = useCallback((nuevo: CartItem) => {
    setItems(prev => {
      const clave = claveItem(nuevo.productId, nuevo.variante?.id ?? null)
      const existente = prev.find(i => claveItem(i.productId, i.variante?.id ?? null) === clave)
      if (existente) {
        return prev.map(i =>
          claveItem(i.productId, i.variante?.id ?? null) === clave
            ? { ...i, cantidad: i.cantidad + nuevo.cantidad }
            : i
        )
      }
      return [...prev, nuevo]
    })
  }, [])

  const quitar = useCallback((productId: number, variantId: number | null | undefined) => {
    const clave = claveItem(productId, variantId)
    setItems(prev => prev.filter(i => claveItem(i.productId, i.variante?.id ?? null) !== clave))
  }, [])

  const setCantidad = useCallback(
    (productId: number, variantId: number | null | undefined, cantidad: number) => {
      const clave = claveItem(productId, variantId)
      setItems(prev =>
        prev
          .map(i => (claveItem(i.productId, i.variante?.id ?? null) === clave ? { ...i, cantidad } : i))
          .filter(i => i.cantidad > 0)
      )
    },
    []
  )

  const vaciar = useCallback(() => setItems([]), [])

  const value = useMemo<CartContextValue>(() => {
    const cantidad = items.reduce((sum, i) => sum + i.cantidad, 0)
    const subtotalCOP = items.reduce(
      (sum, i) => sum + (i.precioBase + (i.variante?.ajusteCOP ?? 0)) * i.cantidad,
      0
    )
    return { items, cantidad, subtotalCOP, agregar, quitar, setCantidad, vaciar }
  }, [items, agregar, quitar, setCantidad, vaciar])

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}

export function useCart() {
  const context = useContext(CartContext)
  if (!context) {
    throw new Error('useCart debe usarse dentro de <CartProvider>')
  }
  return context
}
