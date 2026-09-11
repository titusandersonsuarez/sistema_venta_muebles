export interface UsuarioResumen {
  id: number
  nombreUsuario: string
  nombre: string
  rol: string
}

export interface LoginResponse {
  token: string
  expiraEn: string
  usuario: UsuarioResumen
}
