const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5199/api'
const TOKEN_KEY = 'nogal_token'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

interface RequestOptions extends RequestInit {
  /** Si es true, agrega el header Authorization con el token guardado. */
  auth?: boolean
}

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { auth = false, headers, ...rest } = options
  const finalHeaders = new Headers(headers)
  finalHeaders.set('Content-Type', 'application/json')

  if (auth) {
    const token = localStorage.getItem(TOKEN_KEY)
    if (token) {
      finalHeaders.set('Authorization', `Bearer ${token}`)
    }
  }

  const response = await fetch(`${API_URL}${path}`, {
    ...rest,
    headers: finalHeaders
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
export async function apiUpload<T>(path: string, body: FormData, options: { auth?: boolean } = {}): Promise<T> {
  const headers = new Headers()
  if (options.auth) {
    const token = localStorage.getItem(TOKEN_KEY)
    if (token) {
      headers.set('Authorization', `Bearer ${token}`)
    }
  }

  const response = await fetch(`${API_URL}${path}`, {
    method: 'POST',
    body,
    headers
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

export { TOKEN_KEY, API_URL }
