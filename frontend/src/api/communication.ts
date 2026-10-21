import { apiFetch } from './client'

export function enviarContacto(nombre: string, contacto: string, mensaje: string) {
  return apiFetch<void>('/contact', { method: 'POST', body: JSON.stringify({ nombre, contacto, mensaje }) })
}

export interface ChatResponse { sessionId: string; respuesta: string }

export function enviarChat(sessionId: string, mensaje: string) {
  return apiFetch<ChatResponse>('/chat', { method: 'POST', body: JSON.stringify({ sessionId, mensaje }) })
}
