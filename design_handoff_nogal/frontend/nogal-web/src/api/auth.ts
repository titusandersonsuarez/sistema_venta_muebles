import { apiFetch } from './client'
import type { LoginResponse } from '../types/auth'

export function login(nombreUsuario: string, password: string) {
  return apiFetch<LoginResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ nombreUsuario, password })
  })
}
