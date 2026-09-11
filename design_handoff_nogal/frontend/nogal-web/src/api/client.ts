const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5199/api'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

/**
 * Cliente HTTP común. La sesión viaja en una cookie HttpOnly `nogal_auth`
 * que el navegador adjunta automáticamente en cada request cuando
 * `credentials: 'include'` está activo — el frontend no maneja tokens.
 */
export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const { headers, ...rest } = options
  const finalHeaders = new Headers(headers)
  finalHeaders.set('Content-Type', 'application/json')

  const response = await fetch(`${API_URL}${path}`, {
    ...rest,
    headers: finalHeaders,
    credentials: 'include'
  })

  if (!response.ok) {
    let mensaje = `Error ${response.status}`
    try {
      const data = await response.json()
      mensaje = data?.mensaje ?? mensaje
    } catch {
      // el cuerpo no era JSON; nos quedamos con el mensaje genérico
    }
    throw new ApiError(response.status, mensaje)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

/**
 * Variante para subir archivos: no fija Content-Type (el navegador
 * agrega el boundary correcto de multipart/form-data).
 */
export async function apiUpload<T>(path: string, body: FormData): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    method: 'POST',
    body,
    credentials: 'include'
  })

  if (!response.ok) {
    let mensaje = `Error ${response.status}`
    try {
      const data = await response.json()
      mensaje = data?.mensaje ?? mensaje
    } catch {
      // idem
    }
    throw new ApiError(response.status, mensaje)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export { API_URL }
