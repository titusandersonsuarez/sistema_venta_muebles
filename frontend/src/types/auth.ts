export interface UsuarioResumen {
  id: number
  nombreUsuario: string
  nombre: string
  rol: string
}

export interface LoginResponse {
  expiraEn: string
  usuario: UsuarioResumen
}
