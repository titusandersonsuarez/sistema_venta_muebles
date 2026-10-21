export interface ProductionStage {
  nombre: string
  ordenes: number
  diasPromedio: number
}

export interface ProductionSummary {
  etapas: ProductionStage[]
  capacidadMensual: number
  comprometido: number
}

export interface InventoryItem {
  id: number
  nombre: string
  stock: string
  estado: 'Crítico' | 'Bajo' | 'Normal'
}
