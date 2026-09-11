# Prompt para IA — Continuar funcionalidades de Nogal

Pega este bloque completo en la IA con la que vayas a seguir trabajando (Claude Code u otra). Da contexto del estado actual del proyecto y de qué sigue.

---

## PROMPT

Eres un ingeniero de software full-stack senior. Vas a continuar el desarrollo de **Nogal**, el sistema de venta de muebles de una fábrica en Bogotá (Puente Aranda), sobre un proyecto que **ya tiene código de arranque** — no partas de cero, extiende lo que existe.

### Estado actual del proyecto

- `design_handoff_nogal/` — paquete de diseño (fuente de verdad visual): `Nogal Muebles.dc.html` (prototipo completo, tienda + panel), `styles.css` (tokens y componentes del sistema "Classical"), `sistema-classical.md` (guía de uso) y `README.md` (spec funcional completa de cada pantalla).
- `backend/NogalApi/` — API en ASP.NET Core 9 + EF Core + PostgreSQL. Ya implementado: modelo `Usuario` (usuarios del panel), `AppDbContext`, `AuthService` (BCrypt + JWT), `AuthController` (`POST /api/auth/login`, `GET /api/auth/me`), seed automático de un admin inicial, CORS para el front, Swagger con auth Bearer, `Dockerfile`.
- `frontend/nogal-web/` — React 18 + TypeScript + Vite. Ya implementado: `styles.css` copiado del paquete de diseño (úsalo tal cual, no reinventes tokens), cliente `apiFetch` con manejo de token/errores, `AuthContext` + `RequireAuth`, `LoginPage` (fiel al prototipo), `AdminLayout` (rail lateral con los 4 módulos + topbar) y una página *placeholder* por módulo (Resumen, Pedidos, Productos, Producción) más un `HomePlaceholder` para la tienda pública.
- `docker-compose.yml` en la raíz levanta Postgres + la API.

Antes de escribir código, **lee `design_handoff_nogal/README.md` completo** (ahí está la especificación pantalla por pantalla, el modelo de datos sugerido y los endpoints mínimos) y **revisa el HTML del prototipo** para copiar la disposición y las clases exactas (`.card`, `.table`, `.tag`, `.seg`, `.dialog`, etc.) — no inventes markup nuevo.

### Reglas que no se negocian

1. **Sigue el patrón de capas que ya existe** en `NogalApi` (Models → Data/DbContext → Services (interfaz + implementación) → Controllers) para cada entidad nueva. No mezcles lógica de negocio en los controllers.
2. **Todo estilo sale de `styles.css`** (colores, tipografía, espaciado, radios, sombras vía `var(--...)`) y de las clases ya definidas. Nunca un hex, un px o una fuente sueltos.
3. Moneda **COP**, formato `es-CO` (ej. `$ 1.890.000`). Idioma español colombiano en toda la UI y los mensajes de error.
4. Las imágenes de producto se guardan como **URL** (bucket S3/GCS), nunca como binario en la base de datos.
5. Cada módulo nuevo necesita: migración de EF Core, endpoints REST, y la pantalla React conectada de verdad a la API (no mocks una vez que el backend exista).

### Qué sigue (en este orden)

1. **Productos (CRUD)** — backend: modelo `Product` (Id, Slug, Nombre, Categoria, Material, PrecioCOP, Medidas, Peso, Armado, Descripcion, Estado [Disponible/EnProceso/Vendido], ImagenUrl, Activo) + `ProductImage`, subida de imagen (multipart → bucket, guarda solo la URL), endpoints `GET/POST/PUT/DELETE /api/admin/products` y `GET /api/products` (público, con filtros categoría/material/precioMax). Frontend: reemplaza el placeholder de `ProductosPage` con el formulario "Subir un producto nuevo" y la tabla editable (miniatura, nombre, categoría, precio, descripción, foto, estado, acciones Restaurar/Eliminar) tal como la describe el README.
2. **Catálogo público + ficha de producto** — usa `GET /api/products`: página de catálogo con los filtros controlados por estado (categoría, precio máximo, material) y la ficha de producto (galería, precio, control de acabado, tabla de medidas) consumiendo el mismo endpoint.
3. **Pedidos** — modelo `Order`/`OrderItem`, tabla en el panel (`GET/PATCH /api/admin/orders`) con los estados En ruta / En taller / Entregado / Pago pendiente.
4. **Resumen (dashboard)** — endpoint `GET /api/admin/sales?from=&to=&granularity=` que devuelva *buckets* {label, ventas, pedidos}; en el front, las 4 tarjetas KPI, la gráfica de ventas en el tiempo y la mezcla por categoría, tal como en el prototipo.
5. **Producción** — modelo `ProductionOrder` / `InventoryItem`, tarjetas de etapa y tabla de materiales por reponer.
6. **Home + contacto + chat "Nogalito"** — home pública completa, formulario de contacto (`POST /api/contact`), y el chat: empieza con `POST /api/chat` por palabras clave (igual al prototipo) dejando el contrato listo para enchufar un LLM (Claude API) más adelante usando el catálogo real como contexto.

Trabaja un módulo a la vez, de punta a punta (migración → endpoint → pantalla), y no avances al siguiente hasta que el anterior compile y funcione contra la base de datos real. Si necesitas decidir algo de negocio que el README no defina (zonas de envío, métodos de pago, textos de marketing), pregúntame en vez de inventarlo.

---

*Generado a partir del handoff de diseño `design_handoff_nogal/README.md` y del código ya scaffolded en `backend/NogalApi` y `frontend/nogal-web`.*
