import '@google/model-viewer'
import { API_URL } from '../api/client'

interface ArDialogProps { abierto: boolean; alCerrar: () => void; producto?: string; modelo3dUrl?: string | null; modeloUsdzUrl?: string | null }

export function ArDialog({ abierto, alCerrar, producto, modelo3dUrl, modeloUsdzUrl }: ArDialogProps) {
  if (!abierto) return null
  const modelo = modelo3dUrl ? urlAbsoluta(modelo3dUrl) : null
  const modeloApple = modeloUsdzUrl ? urlAbsoluta(modeloUsdzUrl) : undefined

  return <div className="dialog-backdrop" role="presentation" onMouseDown={alCerrar}><section className="dialog ar-dialog" role="dialog" aria-modal="true" aria-labelledby="ar-dialog-title" onMouseDown={(event) => event.stopPropagation()}><h2 id="ar-dialog-title" className="dialog-title">Ver en tu espacio</h2>{modelo ? <model-viewer className="ar-model" src={modelo} ios-src={modeloApple} crossorigin="anonymous" alt={`Modelo 3D de ${producto ?? 'mueble Nogal'}`} ar ar-modes="webxr scene-viewer quick-look" ar-scale="fixed" ar-placement="floor" camera-controls touch-action="pan-y" shadow-intensity="1" environment-image="neutral" auto-rotate loading="eager"><button slot="ar-button" className="btn btn-primary">Ver en tu espacio</button></model-viewer> : <div className="plate ar-preview"><span>{producto ? `${producto} aún no tiene modelo 3D` : 'Selecciona un producto con modelo 3D'}</span></div>}<p className="dialog-body">{modelo ? 'Gira el modelo para inspeccionarlo. En un dispositivo compatible usa “Ver en tu espacio” para abrir AR.' : 'Cuando cargues el archivo GLB y, opcionalmente, el USDZ para iPhone/iPad desde el panel, aquí podrás verlo y ubicarlo en tu espacio.'}</p><div className="dialog-actions"><button className="btn btn-secondary" onClick={alCerrar}>Cerrar</button></div></section></div>
}

function urlAbsoluta(url: string): string {
  if (/^(https?:|data:|blob:)/i.test(url)) return url
  const base = API_URL.replace(/\/api\/?$/, '')
  return `${base}${url.startsWith('/') ? '' : '/'}${url}`
}
