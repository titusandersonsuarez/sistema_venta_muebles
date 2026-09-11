import { apiFetch } from './client'
import type { LoginResponse, UsuarioResumen } from '../types/auth'

export function login(nombreUsuario: string, password: string) {
  return apiFetch<LoginResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ nombreUsuario, password })
  })
}

export function me() {
  return apiFetch<UsuarioResumen>('/auth/me')
}

export function logout() {
  return apiFetch<void>('/auth/logout', { method: 'POST' })
}
