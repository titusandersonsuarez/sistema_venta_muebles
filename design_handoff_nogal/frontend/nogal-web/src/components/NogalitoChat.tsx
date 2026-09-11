import { FormEvent, useEffect, useRef, useState } from 'react'
import { enviarChat } from '../api/communication'

const SESSION_KEY = 'nogal_chat_session'

interface MensajeChat {
  origen: 'bot' | 'user'
  texto: string
}

export function NogalitoChat() {
  const [chatAbierto, setChatAbierto] = useState(false)
  const [chatTexto, setChatTexto] = useState('')
  const [chat, setChat] = useState<MensajeChat[]>([
    {
      origen: 'bot',
      texto: '¡Hola! Soy Nogalito, el asistente del taller. ¿En qué puedo orientarte hoy sobre nuestros muebles, maderas o entregas?'
    }
  ])
  const [enviandoChat, setEnviandoChat] = useState(false)
  const historyRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (chatAbierto && historyRef.current) {
      historyRef.current.scrollTop = historyRef.current.scrollHeight
    }
  }, [chat, chatAbierto, enviandoChat])

  async function enviarMensaje(event?: FormEvent, sugerido?: string) {
    event?.preventDefault()
    const texto = (sugerido ?? chatTexto).trim()
    if (!texto || enviandoChat) return

    setChat((actual) => [...actual, { origen: 'user', texto }])
    setChatTexto('')
    setEnviandoChat(true)

    try {
      const sessionId = localStorage.getItem(SESSION_KEY) ?? ''
      const respuesta = await enviarChat(sessionId, texto)
      if (respuesta.sessionId) {
        localStorage.setItem(SESSION_KEY, respuesta.sessionId)
      }
      setChat((actual) => [...actual, { origen: 'bot', texto: respuesta.respuesta }])
    } catch {
      setChat((actual) => [
        ...actual,
        {
          origen: 'bot',
          texto: 'Por el momento no pude conectar con el taller. Puedes escribirnos directamente a nuestro WhatsApp 300 000 0000.'
        }
      ])
    } finally {
      setEnviandoChat(false)
    }
  }

  const sugerencias = [
    '¿Cómo funciona el envío gratis?',
    '¿Qué maderas usan en el taller?',
    '¿Tienen servicio de armado?',
    '¿Dónde queda la fábrica en Bogotá?'
  ]

  return (
    <>
      <button
        type="button"
        className="home-chat-trigger btn btn-primary"
        onClick={() => setChatAbierto(true)}
        aria-label="Abrir asistente Nogalito"
        style={{
          boxShadow: 'var(--shadow-md)',
          display: chatAbierto ? 'none' : 'inline-flex',
          alignItems: 'center',
          gap: '8px'
        }}
      >
        <span>💬</span>
        <span>Hablar con Nogalito (IA)</span>
      </button>

      {chatAbierto && (
        <aside className="home-chat" aria-label="Asistente Nogalito">
          <header>
            <div>
              <strong>Nogalito</strong>
              <span>Asistente IA del Taller · en línea</span>
            </div>
            <button
              type="button"
              className="btn btn-ghost"
              onClick={() => setChatAbierto(false)}
              aria-label="Cerrar chat"
            >
              Cerrar
            </button>
          </header>

          <div className="home-chat-history" ref={historyRef}>
            {chat.map((mensaje, indice) => (
              <p
                className={`home-message ${mensaje.origen}`}
                key={indice}
                style={{ whiteSpace: 'pre-line' }}
              >
                {mensaje.texto}
              </p>
            ))}
            {enviandoChat && (
              <p
                className="home-message bot"
                style={{ fontStyle: 'italic', opacity: 0.8 }}
              >
                Nogalito está consultando el taller...
              </p>
            )}
          </div>

          <div className="home-chat-suggestions">
            {sugerencias.map((texto) => (
              <button
                key={texto}
                type="button"
                onClick={() => enviarMensaje(undefined, texto)}
                disabled={enviandoChat}
              >
                {texto}
              </button>
            ))}
          </div>

          <form onSubmit={enviarMensaje}>
            <input
              className="input"
              value={chatTexto}
              onChange={(e) => setChatTexto(e.target.value)}
              placeholder="Pregúntame sobre un mueble o madera..."
              disabled={enviandoChat}
              autoFocus
            />
            <button
              className="btn btn-primary"
              type="submit"
              disabled={enviandoChat || !chatTexto.trim()}
            >
              Enviar
            </button>
          </form>
        </aside>
      )}
    </>
  )
}
