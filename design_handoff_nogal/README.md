# Handoff: Nogal — tienda de muebles + panel de fábrica (Bogotá)

## Overview
Nogal es una **fábrica de muebles en Bogotá (Puente Aranda)** que vende directo al consumidor.
El diseño cubre dos productos en un mismo dominio:

1. **Tienda pública** — home, catálogo con filtros, ficha de producto, contacto/ubicación y un asistente de chat.
2. **Panel interno (admin)** — login, resumen de ventas con gráficas interactivas por fechas, pedidos, CRUD de productos y estado de producción/inventario.

Moneda: **COP** (formato `$ 1.890.000`, es-CO). Idioma: español colombiano.

## Arquitectura implementada

El proyecto actual utiliza una arquitectura sencilla por responsabilidades:

```text
React + TypeScript + Vite
      ↓
    Controllers
      ↓
       DTOs
      ↓
    IService
      ↓
     Services
      ↓
 Entity Framework Core / AppDbContext
      ↓
 SQL Server .\\SQLEXPRESS / NogalDb
```

- Los **Controllers** gestionan routing, HTTP, autorización, DTOs y códigos de estado.
- Las interfaces `I*Service` definen los contratos de aplicación.
- Los **Services** contienen las reglas de negocio, consultas, paginación, mapeo a DTOs y persistencia mediante EF Core.
- `AppDbContext` configura entidades, relaciones, índices y restricciones.
- Los DTOs evitan exponer directamente las entidades de Entity Framework en la API pública.
- No se utiliza Repository Pattern: EF Core ya se consume desde los Services y no existe una necesidad técnica adicional.

### Backend actual

- ASP.NET Core .NET 9 y Entity Framework Core.
- SQL Server Express local: `Server=.\\SQLEXPRESS;Database=NogalDb;Trusted_Connection=True;TrustServerCertificate=True;`.
- Migraciones automáticas al iniciar con `Database.MigrateAsync()`.
- JWT con BCrypt y rol `Admin` para endpoints administrativos.
- Almacenamiento local de imágenes mediante `IProductImageStorage`.
- Swagger/OpenAPI, CORS y archivos estáticos configurados en `Program.cs`.

### Refactorización aplicada

- Las validaciones de nombre, precio, categoría, material y estado de productos viven en `ProductService`.
- `AdminProductsController` traduce los errores de validación a respuestas HTTP `400` sin contener las reglas de negocio.
- Los endpoints de productos y pedidos administrativos requieren `[Authorize(Roles = "Admin")]`.
- La carga de imágenes valida extensión, `Content-Type` y firma binaria básica, manteniendo el almacenamiento local y el campo multipart `archivo`.
- Se conservaron las rutas, DTOs y nombres de propiedades consumidos por React.

### Dashboard implementado

El resumen del panel ya consume datos reales mediante `GET /api/admin/sales`.

- Parámetros obligatorios: `from` y `to` en formato `yyyy-MM-dd`; `to` es exclusivo.
- `granularity` admite `day`, `week` y `month`.
- Las fechas se interpretan como calendario de Bogotá y se convierten a UTC antes de consultar SQL Server.
- Los pedidos con estado `Pago pendiente` no se consideran ventas confirmadas.
- La respuesta incluye totales de ventas, pedidos, unidades y ticket promedio; buckets temporales; mezcla por categoría; y los cinco productos más vendidos.
- La pantalla `/admin` incluye atajos de rango, filtros, KPI, barras seleccionables, mezcla por categoría y ranking de productos.

### AR 3D implementado

Cada producto puede guardar dos URLs opcionales:

- `Modelo3dUrl`: modelo `.glb` o `.gltf` para el visor 3D y Android/WebXR.
- `ModeloUsdzUrl`: modelo `.usdz` opcional para Quick Look en iPhone/iPad.

El panel de Productos permite editar ambas URLs. La ficha pública abre un visor oficial `@google/model-viewer` con controles de cámara, rotación automática y AR mediante `webxr`, `scene-viewer` y `quick-look`. Si el producto no tiene modelo, muestra un estado informativo y no rompe la ficha.

Los archivos 3D deben estar publicados en una URL accesible por el navegador, con CORS configurado para el frontend. La API solo guarda las URLs; no almacena modelos binarios en SQL Server. La migración `20260911183658_AddProduct3dModels` crea las columnas correspondientes.

### Flujo de generación automática

Al subir una imagen desde el panel, el producto queda en estado `Pendiente` y se encola un trabajo de generación. La API procesa el trabajo en segundo plano y conserva los estados `Sin modelo`, `Pendiente`, `Procesando`, `Disponible` o `Error`. Los trabajos pendientes se recuperan después de reiniciar la API.

- `POST /api/admin/products/{id}/3d-generation` permite reintentar manualmente.
- La imagen de referencia debe existir antes de solicitar la generación.
- La generación real requiere conectar un proveedor image-to-3D externo mediante `Product3d`; no se fabrica un GLB falso a partir de una sola imagen.
- `Product3d:Enabled` está desactivado por defecto. La cola, estados y contrato ya están preparados; el adaptador del proveedor debe recibir una API key y devolver las URLs finales GLB/USDZ.
- La migración `AddProduct3dGenerationStatus` guarda el estado, el error y la fecha de solicitud.

Producción e inventario, la home pública, contacto y el chat Nogalito ya están implementados.

### Comandos de verificación

Desde `backend`:

```powershell
dotnet build .\\NogalApi.sln
```

Desde `frontend/nogal-web`:

```powershell
npm install
npm run build
```

Para ejecutar localmente, inicia la API desde `backend/NogalApi` y el frontend con `npm run dev` desde `frontend/nogal-web`. La cadena de conexión de SQL Server se encuentra en `appsettings.json` y puede sobrescribirse mediante la configuración estándar de ASP.NET Core.

## About the Design Files
Los archivos de este paquete son **referencias de diseño hechas en HTML**: prototipos que muestran el aspecto y el comportamiento deseados, **no código de producción para copiar y pegar**.
La tarea es **recrear estos diseños en el stack objetivo** (aquí: **React + TypeScript** en el front y **ASP.NET Core (C#)** en el back), usando los patrones y librerías de ese proyecto. El HTML sirve como especificación visual y funcional.

## Fidelity
**Alta fidelidad (hifi)** en layout, tipografía, color, copys y comportamiento. Las **fotografías son marcadores** (bloques rayados con una nota de qué foto va allí): deben reemplazarse por imágenes reales.

---

## Design tokens (sistema "Classical")

Todos los valores salen de `styles.css` (incluido en el paquete). No inventar colores ni tamaños fuera de esta lista.

### Color
| Token | Valor | Uso |
|---|---|---|
| `--color-bg` | `#f3f2f2` | fondo de página |
| `--color-surface` | `#eae9e9` | superficies (diálogo, marco de fotos) |
| `--color-text` | `#201f1d` | texto |
| `--color-accent` | `#b68235` | acento (solo trazo/borde/texto, nunca relleno grande) |
| `--color-divider` | `rgba(32,31,29,0.16)` | hairlines |
| `--color-accent-100…900` | `#fff3e4 … #3a270d` | rampa del acento (tintes, barras de gráficas, estados) |
| `--color-neutral-100…900` | `#f8f4f4 … #2d2b2b` | rampa neutra |

Texto en acento a tamaño de párrafo: usar `--color-accent-700` (contraste).

### Tipografía
- Títulos: **Cormorant Garamond** (400 para display, 600 como tope en UI).
- Cuerpo: **Lora** 400. Base 15px / line-height 1.55.
- Escala: h1 42px (hero hasta `clamp(40px,5.4vw,64px)`), h2 32px, h3 25px, h4 20px, h6 13px uppercase letter-spacing .08em.
- Cifras: `font-variant-numeric: tabular-nums` en precios, KPIs, tablas y ejes.
- Nada de negrita fuerte ni sans-serif para énfasis (usar itálica).

### Espaciado / radios / sombras
- `--space-1..8`: 4.6 / 9.2 / 13.8 / 18.4 / 27.6 / 36.8 px.
- `--radius-sm|md|lg`: 2 / 4 / 7 px.
- `--shadow-sm|md|lg`: sombras muy suaves (elevación mínima).

### Componentes de la hoja de estilos
`.btn` (+`.btn-primary` borde acento sin relleno, `.btn-secondary`, `.btn-ghost`, `.btn-block`), `.tag` (+`-accent`, `-outline`, `-neutral`), `.input`, `.field`, `.radio`+`.dot`, `.seg`+`.seg-opt`, `.card` (+`-kicker`, `-title`, `-body`, `-meta`), `.nav`+`.nav-brand`, `.table`, `.dialog-backdrop`+`.dialog`, `.hr`, `.plate` (marco de fotografías: borde 6px `--color-surface`, outline hairline, filtro `sepia(.22) saturate(.82) contrast(1.05)`).

Estados: hover con tinte del acento, `:focus-visible { outline: 2px solid var(--color-accent); outline-offset: 2px }`, disabled 45% opacidad.

---

## Screens / Views

### 1. Header (global, tienda)
Sticky, `.nav`, hairline inferior, padding lateral `max(--space-4, 4vw)`, `flex-wrap: wrap`.
Marca "Nogal" (Cormorant 18px) + enlaces: Inicio, Catálogo, La fábrica, Admin, separador hairline, botón secundario **"Carrito · N"**.

### 2. Home
1. **Hero** — grid `auto-fit minmax(320px,1fr)`, gap `--space-8`.
   Izquierda: kicker "Fábrica propia en Bogotá · Entrega en 5 días hábiles"; h1 "Somos la fábrica, no el intermediario."; párrafo 17px; botones "Ver el catálogo" (primary) y "Verlo en tu casa" (secondary); tres cifras (4,8/5 · 2.140 reseñas / 10 años garantía / Gratis envío desde $ 500.000).
   Derecha: `.plate` 4:3.2 con marcador de foto.
2. **Por espacio** — 4 tarjetas de categoría (Sofás, Sillas, Mesas, Camas) con `.plate` 3:3.4, nombre y "desde $ X" (mínimo real de la categoría). Clic → catálogo filtrado por esa categoría.
3. **Los más pedidos este mes** — 4 productos (grid `auto-fit minmax(230px,1fr)`): foto, nombre, precio, "★★★★★ · N reseñas · Despacho en X días".
4. **Ver en tu espacio (AR)** — dos columnas: texto + botón "Probar con la cámara" (abre modal) y `.plate` 16:10.
5. **Reseñas** — 3 citas con borde izquierdo acento, estrellas, texto en itálica, autor · producto.
6. **Servicios** — 4 columnas: Envío gratis, Armado opcional ($ 89.000), 30 días para pensarlo, Paga a cuotas (tarjeta, PSE, Addi, Sistecrédito).
7. **Contáctanos / Dónde estamos** — formulario (nombre, celular o correo, mensaje, botón "Enviar mensaje") + `.plate` 16:10 para el mapa y tabla con Dirección (Cra. 56 #17-40, Puente Aranda, Bogotá), Horario (L-V 8:00–17:00, Sáb 8:00–12:00), WhatsApp 300 000 0000, correo hola@nogal.com.co.
8. **Footer** — 4 columnas: marca + dirección, Comprar, Ayuda, Novedades (input + botón).

### 3. Catálogo
Breadcrumb, h1 "Catálogo", conteo "N muebles fabricados en Bogotá, listos para despacho", hairline.
Layout: `display:flex; flex-wrap:wrap; gap:--space-8`; **aside** `flex:1 1 200px; max-width:240px`; **grid** `flex:100 1 260px` con `repeat(auto-fill, minmax(200px,1fr))`.
Filtros (todos controlados por estado):
- Categoría: radios (Todos, Sofás, Sillas, Mesas, Camas).
- Precio máximo: `range` 150.000 → 2.500.000 paso 50.000, etiqueta "hasta $ X".
- Material: radios (Todos, Madera maciza, Tapizado, Metal y madera).
- Botón "Limpiar filtros".
Tarjeta de producto: `.plate` 4:3.4, nombre, precio, "material · medidas", estrellas + reseñas.
Estado vacío: caja con borde, "Nada con esos filtros" + botón limpiar.

### 4. Ficha de producto
Dos columnas `auto-fit minmax(320px,1fr)`:
- Galería: `.plate` principal 4:3.2 + 3 miniaturas 1:1.
- Info: kicker "<Categoría> · hecho en nuestro taller", h1, precio 34px + "IVA incluido · o $ X al mes en 12 cuotas", estrellas + rating + reseñas, descripción justificada, control segmentado **Acabado** (Roble natural, Nogal oscuro, Lino crudo, Gris piedra), botones "Agregar al carrito" (primary) y "Ver en tu espacio", línea de envío/devolución, tabla Medidas/Material/Peso/Armado/Garantía.
Debajo: **Reseñas** (3, con estrellas, fecha, texto, autor) y **Combina bien con** (3 productos de otras categorías).

### 5. Modal AR
`.dialog-backdrop` + `.dialog` (máx 520px): título "Ver en tu espacio", `.plate` 3:4 (marcador cámara + modelo 3D), texto explicativo, acciones "Cerrar" / "Abrir la cámara". Cierra al hacer clic en el fondo.

### 6. Chat "Nogalito" (tienda)
Botón fijo abajo-derecha "Hablar con nosotros". Panel 340px: encabezado (nombre + "en línea" + cerrar), historial con burbujas (usuario alineado a la derecha con fondo `--color-accent-100` y borde `--color-accent-300`; bot a la izquierda, transparente, borde divider), chips de preguntas sugeridas, input + "Enviar".
Respuestas por palabras clave: envío/entrega, precios, garantía, armado, ubicación/taller, pagos/cuotas, devoluciones, medidas/personalización, AR, saludo; fallback → WhatsApp.
> En producción esto debe pasar a un endpoint real (ver "Backend").

### 7. Login del panel
Tarjeta centrada 380px: kicker "Acceso interno", h2 "Panel de la fábrica", campos Usuario y Contraseña, mensaje de error en acento con borde izquierdo, botón "Entrar" `.btn-block`.
Demo del prototipo: `admin` / `nogal2026` (reemplazar por auth real).

### 8. Panel — shell
`main` flex: **rail lateral** (`flex:1 1 210px; max-width:236px`, borde derecho hairline, sticky top 64px) con marca "Nogal / Panel de fábrica", lista de módulos (Resumen, Pedidos, Productos, Producción) con contador a la derecha y activo = borde acento + fondo `--color-accent-100`, y abajo "Ver la tienda" / "Cerrar sesión".
**Contenido** (`flex:100 1 340px`): barra superior con título y subtítulo del módulo, buscador, "Exportar CSV" y avatar circular (iniciales en borde acento) + nombre/rol.

### 9. Panel — Resumen
- Atajos: Hoy, 7 días, 30 días, 90 días (fijan rango y granularidad).
- **4 tarjetas KPI**: título, variación (`.tag-outline`), valor 30px, sparkline de 14 barras, nota.
- **Ventas en el tiempo**: inputs `date` Desde/Hasta, segmentado Día/Semana/Mes, barras (alto proporcional, `title` con "etiqueta · valor · pedidos"), clic fija la barra y muestra tira de detalle (etiqueta, días, ventas, pedidos, ticket) con "Quitar selección".
- **Mezcla por categoría**: anillo `conic-gradient` (Sofás 46%, Mesas 22%, Camas 20%, Sillas 12%) con centro en color de fondo y total; leyenda con cuadro de color, %, valor.
- **Más vendidos del periodo**: 5 filas con nombre, "N und · $ X" y barra de progreso hairline.

### 10. Panel — Pedidos
Tabla `.table` con Pedido, Cliente, Ciudad, Producto, Valor (derecha, tabular), Estado (`.tag`: En ruta = outline, En taller = accent, Entregado = neutral, Pago pendiente = outline). Contenedor `overflow-x:auto`.

### 11. Panel — Productos (CRUD)
- **Formulario "Subir un producto nuevo"**: nombre, categoría (select), material (select), precio (number, paso 10.000), medidas, descripción (textarea), foto (file, preview) y "Publicar producto". Validación: nombre y precio obligatorios.
- **Tabla**: miniatura + botón "Subir" (file), nombre (input), categoría, precio (number), descripción (textarea), estado de imagen + precio formateado, acciones "Restaurar" (vuelve a los valores base) y "Eliminar".
- Fila de eliminados con chips "nombre · devolver".
- Toast/tag de confirmación en el encabezado ("Precio de X actualizado a $ …").
- Todo cambio se refleja de inmediato en la tienda (precio, foto, nombre, descripción, "desde" por categoría).

### 12. Panel — Producción
Cuatro tarjetas de etapa (Corte 14, Armado 22, Tapicería 9, Acabado y empaque 11; "órdenes · X días prom."), nota de capacidad (180 unidades/mes, 142 comprometido) y tabla "Materiales por reponer" con estado en `.tag` (Crítico / Bajo / Normal).

---

## Interactions & Behavior
- Navegación entre vistas sin recarga; al cambiar de vista, `scrollTo(0,0)`.
- Los controles de radio/segmentados son **controlados por estado** (no `checked` estático).
- Catálogo: filtros combinables (categoría AND material AND precio ≤ máximo).
- Gráfica: hover = tooltip nativo; clic = fija/desfija barra; cambiar rango o granularidad limpia la selección.
- Carrito: contador en el header (sin flujo de pago en el prototipo).
- Responsive: todo reflúye por `flex-wrap`/`auto-fit`; tablas anchas en `overflow-x:auto`; el rail del panel se apila bajo ~700px.
- Accesibilidad: foco visible con anillo acento; `label` asociado a cada campo; contraste mínimo 4.5:1 en texto de párrafo.

## State Management (prototipo → producción)
Estado del prototipo (referencia de qué necesita la app):
`vista`, `sec` (módulo del panel), `cat`, `mat`, `precioMax`, `sel` (producto), `carrito`, `acabado`, `ar`, `chatAbierto`/`chat`/`chatTexto`, `usuario`/`clave`/`autenticado`, `desde`/`hasta`/`gran`/`barra`, y los overrides de admin (`precios`, `imgs`, `descs`, `nombres`, `nuevos`, `eliminados`).
En producción: filtros y rango de fechas en la URL (query params), datos del servidor con React Query/SWR, sesión con cookie httpOnly, carrito en servidor o localStorage.

## Arquitectura sugerida (React + .NET)
- **Front**: React 18 + TypeScript, Vite; React Router; TanStack Query para datos; react-hook-form + zod para formularios; Recharts (o barras propias como en el prototipo) para gráficas; CSS Modules o Tailwind mapeado a los tokens de `styles.css`.
- **Back**: ASP.NET Core 8 Web API (controllers o minimal APIs), EF Core + SQL Server/PostgreSQL, ASP.NET Core Identity + JWT (o cookies) con rol `Admin`, subida de imágenes a almacenamiento de objetos (Azure Blob/S3) guardando solo la URL, FluentValidation, Serilog.
- **Endpoints mínimos**:
  - `GET /api/products` (filtros: categoría, material, precioMax, paginación) · `GET /api/products/{slug}`
  - `POST/PUT/DELETE /api/admin/products` · `POST /api/admin/products/{id}/image` (multipart)
  - `GET /api/admin/sales?from=&to=&granularity=day|week|month` (rol `Admin`) → dashboard con `totales`, `buckets`, `categorias` y `masVendidos`
  - `GET /api/admin/orders` · `PATCH /api/admin/orders/{id}/status`
  - `GET /api/admin/production` · `GET /api/admin/inventory`
  - `POST /api/admin/products/{id}/3d-generation` (rol `Admin`) → encola generación 3D para la imagen del producto
  - `POST /api/contact` · `POST /api/chat` (bot; empezar con reglas, luego IA)
  - `POST /api/auth/login` · `POST /api/auth/logout`
- **Modelo de datos**: Product(Id, Slug, Nombre, Categoria, Material, PrecioCOP, Medidas, Peso, Armado, Descripcion, ImagenUrl, Activo), ProductImage, Order(Id, Codigo, Cliente, Ciudad, Total, Estado, CreatedAt), OrderItem, Review, ProductionOrder(Etapa, Producto, DiasEnEtapa), InventoryItem(Nombre, Stock, Estado), ContactMessage, User.
- **Pendientes de producto**: pasarela de pago (Wompi/Mercado Pago/PayU), facturación electrónica DIAN, WhatsApp Business API, carga gestionada de modelos 3D a almacenamiento de objetos y envíos/cobertura por ciudad.

## Assets
- Todas las imágenes del prototipo son **marcadores**; la nota en cada uno indica qué foto va allí (ej. "sofá liena — 3/4 en sala", "mapa · Puente Aranda, Bogotá").
- Iconos: el sistema indica **Lucide**; el prototipo usa etiquetas de texto en su lugar.
- Fuentes: Cormorant Garamond y Lora (Google Fonts).

## Files
- `Nogal Muebles.dc.html` — prototipo completo (tienda + panel). Abre en el navegador.
- `styles.css` — hoja de tokens y componentes del sistema "Classical" (fuente de verdad visual).
- `sistema-classical.md` — guía escrita del sistema de diseño (dirección, do/don't).
