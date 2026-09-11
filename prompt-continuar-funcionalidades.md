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

---

## ESTADO ACTUAL (última actualización: 2026-09-11)

> Esta sección se actualiza en cada sesión para que el próximo Claude (o tú) sepa exactamente qué corre, qué falta y en qué punto se dejó el trabajo. **No borrar; solo mantener al día.**

### Ubicación del proyecto

- Working directory raíz: `C:\Users\USUARIO\Desktop\definitivo\`
- Paquete Nogal (código nuevo): `C:\Users\USUARIO\Desktop\definitivo\design_handoff_nogal\`
- Convive con proyecto viejo (`EntrevistaApi/` + `EntrevistaDesktop/`, WPF) — no relacionado con Nogal, ignorar.

### Qué hace hoy el software

**Panel interno de la fábrica (implementado y funcional contra BD real):**

- **Login** con JWT (BCrypt para el hash, expiración 8h). Admin sembrado automáticamente al primer arranque: `admin` / `nogal2026`.
- **Shell del panel** con rail lateral (Resumen, Pedidos, Productos, Producción), topbar, `RequireAuth` que redirige a login si no hay sesión.
- **Productos (CRUD completo)**:
  - Formulario "Subir un producto nuevo" con nombre, categoría, material, precio (paso 10.000), medidas, descripción y foto opcional.
  - Tabla editable con miniatura, nombre, categoría, precio (formato COP en vivo), descripción, estado (Disponible / EnProceso / Vendido).
  - Guardar cambios explícito por fila (patrón borrador → Guardar), soft delete y restaurar desde chips de eliminados.
  - Subida de foto multipart con guardado en disco (`wwwroot/uploads/products/`) — la BD solo guarda la URL relativa.
  - Endpoint público con filtros (`categoria`, `material`, `precioMax`, paginación) listo, pero sin UI todavía.

**Placeholders (funcionan visualmente pero sin lógica):** Resumen, Pedidos, Producción, Home pública, ficha de producto, chat "Nogalito".

### Qué falta (roadmap ordenado)

- [x] **0. Auth + scaffold del panel**
- [x] **1. Productos (CRUD admin + endpoint público)**
- [x] **2. Catálogo público + ficha de producto** — CERRADO el 2026-09-11.
  - `layouts/StoreLayout.tsx` — header sticky con nav (Inicio / Catálogo / La fábrica / Admin / Carrito · 0) que envuelve todas las rutas públicas.
  - `pages/store/CatalogoPage.tsx` — breadcrumb + h1 + conteo dinámico + aside con radios categoría, range precio (150k–2.5M step 50k), radios material, botón "Limpiar filtros". Grid `auto-fill minmax(200px, 1fr)` con tarjetas `.plate` 4:3.4. Estado vacío con CTA. Los filtros son estado controlado y llaman `GET /api/products` en cada cambio.
  - `pages/store/ProductoPage.tsx` — breadcrumb, dos columnas (galería `.plate` 4:3.2 + 3 miniaturas 1:1 / info kicker + h1 + precio 34px + descripción justificada). Segmentado de Acabado (Roble natural / Nogal oscuro / Lino crudo / Gris piedra) — solo UI local (la BD no tiene variantes de acabado por producto todavía). Tabla Medidas/Material/Peso/Armado/Garantía. Sección "Combina bien con" con 3 productos de otras categorías.
  - `pages/store/HomePlaceholder.tsx` — reescrito con la sección hero del prototipo (kicker, h1 "Somos la fábrica...", CTA "Ver el catálogo", 3 cifras). Home completa (por espacio, más pedidos, servicios, reseñas, footer) queda para módulo #6.
  - `App.tsx` — rutas `/`, `/catalogo`, `/producto/:slug` envueltas por `<StoreLayout />`. `/admin/*` sigue igual con `AdminLayout` + `RequireAuth`.
  - `api/products.ts` — agregado `obtenerPorSlug(slug)`.
  - Verificación real: catálogo sin filtros, filtro por categoría, por material+precioMax, filtro imposible (estado vacío OK), ficha por slug, 404 en slug inexistente, frontend sirviendo HTML. 7/7 pasan.
- [ ] **3. Pedidos** — modelos `Order` + `OrderItem`, endpoints `GET /api/admin/orders` y `PATCH /api/admin/orders/{id}/status`. Tabla en el panel con estados En ruta / En taller / Entregado / Pago pendiente (tags con clases `.tag-outline`, `.tag-accent`, `.tag-neutral`).
- [ ] **4. Resumen (dashboard)** — endpoint `GET /api/admin/sales?from=&to=&granularity=day|week|month` devolviendo buckets `{label, ventas, pedidos}`. En el front: 4 tarjetas KPI con sparklines, gráfica de barras interactiva (clic fija selección), anillo `conic-gradient` de mezcla por categoría, top 5 más vendidos con barra hairline.
- [ ] **5. Producción** — modelos `ProductionOrder` + `InventoryItem`. Tarjetas de 4 etapas (Corte, Armado, Tapicería, Acabado y empaque) + tabla "Materiales por reponer" con estado Crítico/Bajo/Normal.
- [ ] **6. Home pública + contacto + chat "Nogalito"** — home con hero, tarjetas por espacio, más pedidos, servicios, reseñas y footer. Formulario `POST /api/contact`. Chat con `POST /api/chat` empezando por reglas (palabras clave) y dejando el contrato listo para enchufar Claude API con el catálogo real como contexto.

### Diagrama de base de datos

Estado actual + planeado. Se puede ver renderizado en cualquier vista Markdown con soporte Mermaid (VS Code, GitHub, Obsidian).

```mermaid
erDiagram
    Usuarios {
        int Id PK
        nvarchar NombreUsuario UK "80, único"
        nvarchar PasswordHash "BCrypt"
        nvarchar Nombre "120"
        nvarchar Rol "Admin | Operario"
        bit Activo
        datetime2 FechaCreacion
    }

    Products {
        int Id PK
        nvarchar Slug UK "160, único"
        nvarchar Nombre "200"
        nvarchar Categoria "40 — Sofás/Sillas/Mesas/Camas"
        nvarchar Material "40 — Madera maciza/Tapizado/Metal y madera"
        decimal PrecioCOP "12,2"
        nvarchar Medidas "120"
        nvarchar Peso "40"
        nvarchar Armado "120"
        nvarchar Descripcion "MAX"
        nvarchar Estado "30 — Disponible/EnProceso/Vendido"
        nvarchar ImagenUrl "500"
        bit Activo "soft delete"
        datetime2 FechaCreacion
        datetime2 FechaActualizacion
    }

    ProductImages {
        int Id PK
        int ProductId FK
        nvarchar Url "500"
        int Orden
    }

    Orders {
        int Id PK
        nvarchar Codigo UK "20 — NGL-000123"
        nvarchar Cliente "120"
        nvarchar Ciudad "80"
        decimal Total "12,2"
        nvarchar Estado "30 — En ruta/En taller/Entregado/Pago pendiente"
        datetime2 CreatedAt
    }

    OrderItems {
        int Id PK
        int OrderId FK
        int ProductId FK
        int Cantidad
        decimal PrecioUnitario "12,2"
        decimal Subtotal "12,2"
    }

    Reviews {
        int Id PK
        int ProductId FK
        nvarchar Autor "120"
        int Rating "1 a 5"
        nvarchar Texto "MAX"
        datetime2 FechaCreacion
    }

    ProductionOrders {
        int Id PK
        int ProductId FK
        nvarchar Etapa "30 — Corte/Armado/Tapicería/Acabado y empaque"
        int Cantidad
        int DiasEnEtapa
        datetime2 FechaEntrada
    }

    InventoryItems {
        int Id PK
        nvarchar Nombre "120"
        nvarchar Stock "40 — texto libre"
        nvarchar Estado "30 — Crítico/Bajo/Normal"
    }

    ContactMessages {
        int Id PK
        nvarchar Nombre "120"
        nvarchar Contacto "120 — celular o correo"
        nvarchar Mensaje "MAX"
        bit Atendido
        datetime2 FechaCreacion
    }

    ChatSessions {
        int Id PK
        nvarchar SessionId UK "40 — cookie o generado"
        datetime2 CreatedAt
    }

    ChatMessages {
        int Id PK
        int ChatSessionId FK
        nvarchar Origen "10 — user/bot"
        nvarchar Texto "MAX"
        datetime2 FechaCreacion
    }

    Products ||--o{ ProductImages : "tiene"
    Products ||--o{ OrderItems : "aparece en"
    Products ||--o{ Reviews : "recibe"
    Products ||--o{ ProductionOrders : "se produce como"
    Orders ||--o{ OrderItems : "contiene"
    ChatSessions ||--o{ ChatMessages : "acumula"
```

**Alternativa en texto** (por si no tenés Mermaid a mano):

```
IMPLEMENTADAS (migraciones aplicadas 2026-09-11)
  Usuarios ─────────────────────────── login del panel (JWT)
                                        └ NombreUsuario UQ, PasswordHash BCrypt

  Products ─────────────────────────── catálogo real
    │                                   └ Slug UQ, Estado, Activo (soft delete)
    └── 1:N ── ProductImages ────────── galería + imagen principal
                                        └ Url, Orden, CASCADE DELETE

PLANEADAS (roadmap #3–#6)
  Orders ───────────────────────────── panel de pedidos (roadmap #3)
    │                                   └ Estado: En ruta/En taller/Entregado/Pago pendiente
    └── 1:N ── OrderItems ──────────── FK → Products (RESTRICT recomendado)

  Reviews ──────────────────────────── FK → Products (roadmap #2 — ficha de producto)
  ProductionOrders ─────────────────── FK → Products (roadmap #5 — producción)
  InventoryItems ───────────────────── independiente (roadmap #5 — materiales por reponer)
  ContactMessages ──────────────────── formulario "Contáctanos" (roadmap #6)

  ChatSessions ─────────────────────── memoria del chat "Nogalito" (roadmap #6, opcional)
    └── 1:N ── ChatMessages ────────── historial user/bot para conectar a LLM

  __EFMigrationsHistory ────────────── auto (EF Core, no tocar)
```

**Decisiones de esquema ya cerradas:**
- Enums como columnas `nvarchar` con validación en el service, NO como `int` con lookup table. Razón: la UI usa los mismos strings del prototipo (`Sofás`, `Madera maciza`) sin conversión, y agregar categorías nuevas es un `INSERT` conceptual, no una migración.
- Precios en `decimal(12,2)` (permite hasta $ 9.999.999.999,99 COP — suficiente).
- Soft delete via `Activo bit`. Nada de eliminación física, para conservar historial de pedidos que apunten a productos "eliminados".

### Cambio de motor de BD: Postgres → SQL Server

El prompt original y el README sugerían Postgres, pero el usuario ya tenía **SQL Server Express** corriendo local (`.\SQLEXPRESS`, puerto 1433) para `EntrevistaApi`. Reemplazamos el provider en esta sesión:

- `NogalApi.csproj`: `Npgsql.EntityFrameworkCore.PostgreSQL` → `Microsoft.EntityFrameworkCore.SqlServer 9.0.4`.
- `Program.cs`: `UseNpgsql` → `UseSqlServer`.
- `appsettings.json`: `Server=.\SQLEXPRESS;Database=NogalDb;Trusted_Connection=True;TrustServerCertificate=True;`.
- BD `NogalDb` creada por `MigrateAsync()` al arrancar.

Migraciones aplicadas:
- `20260911160600_InitialUsuarios` — tabla `Usuarios`.
- `20260911162006_AddProducts` — tablas `Products`, `ProductImages`, índice único en `Slug`, cascade delete.

Docker Postgres del `docker-compose.yml` original **no se usa** (Docker Desktop apagado + puerto 5432 ya ocupado por Postgres locales del usuario). El compose queda como referencia histórica.

### Cómo arrancar

```powershell
# Backend en http://localhost:5199
cd design_handoff_nogal\backend\NogalApi
$env:ASPNETCORE_URLS = "http://localhost:5199"
dotnet run --no-launch-profile

# Frontend en http://localhost:5173 (en otra terminal)
cd design_handoff_nogal\frontend\nogal-web
npm run dev
```

Login del panel: `admin` / `nogal2026`. Swagger disponible en `http://localhost:5199/swagger`.

### Detalle de lo implementado (para ubicarse rápido en el código)

**Módulo 0 — Auth + scaffold**
- Backend: `Models/Usuario.cs`, `Models/Auth/{LoginRequest,LoginResponse}.cs`, `Data/{AppDbContext,DbSeeder}.cs`, `Options/{JwtOptions,AdminSeedOptions}.cs`, `Services/{IAuthService,AuthService}.cs`, `Controllers/AuthController.cs`.
- Frontend: `api/{client,auth}.ts`, `auth/{AuthContext,RequireAuth}.tsx`, `layouts/AdminLayout.tsx`, `pages/admin/LoginPage.tsx`, placeholders `{Resumen,Pedidos,Productos,Produccion}Page.tsx`, `pages/store/HomePlaceholder.tsx`, `src/vite-env.d.ts` (fix `import.meta.env`).

**Módulo 1 — Productos**
- Backend: `Models/Product.cs`, `Models/ProductImage.cs`, `Models/Products/{ProductCatalogo,ProductDtos}.cs`, `Services/{IProductService,ProductService}.cs`, `Services/{IProductImageStorage,LocalProductImageStorage}.cs`, `Options/ImageStorageOptions.cs`, `Controllers/{AdminProductsController,ProductsController,CatalogController}.cs`, `wwwroot/uploads/products/`.
- Frontend: `types/product.ts`, `api/products.ts`, `pages/admin/ProductosPage.tsx` (reescrito completo), `api/client.ts` (agregado `apiUpload`).

### Endpoints activos hoy

| Método | Ruta | Auth | Qué hace |
|---|---|---|---|
| POST | `/api/auth/login` | público | Devuelve JWT + datos del usuario |
| GET | `/api/auth/me` | Bearer | Datos del usuario autenticado |
| POST | `/api/auth/logout` | Bearer | No-op (JWT es sin estado — placeholder para blacklist futura) |
| GET | `/api/catalog/options` | público | Listas de categorías, materiales y estados |
| GET | `/api/products` | público | Listar con filtros `categoria`, `material`, `precioMax`, `pagina`, `tamano` |
| GET | `/api/products/{slug}` | público | Ficha por slug |
| GET | `/api/admin/products` | Bearer | Listar admin (incluye inactivos si `?incluirInactivos=true`) |
| GET | `/api/admin/products/{id}` | Bearer | Obtener por Id |
| POST | `/api/admin/products` | Bearer | Crear |
| PUT | `/api/admin/products/{id}` | Bearer | Actualizar |
| DELETE | `/api/admin/products/{id}` | Bearer | Soft delete (`Activo=false`) |
| POST | `/api/admin/products/{id}/restore` | Bearer | Restaurar |
| POST | `/api/admin/products/{id}/image` | Bearer (multipart) | Subir imagen (JPG/PNG/WEBP/GIF, máx 5 MB) |
| GET | `/uploads/products/{archivo}` | público | Servido por `UseStaticFiles` |

### Decisiones tomadas en esta sesión

- **Motor BD**: SQL Server Express local en vez del Postgres del prompt (razón: el usuario ya lo tiene funcionando con `EntrevistaApi`).
- **Imágenes**: disco local (`wwwroot/uploads/products/`) con archivos nombrados por GUID. Interfaz `IProductImageStorage` para swapear a S3/GCS más adelante sin tocar el resto del código.
- **Categoría / Material / Estado**: listas fijas en `ProductCatalogo.cs` (backend) que el front consume vía `/api/catalog/options`. NO enums. NO tabla lookup.
- **Slug**: autogenerado del nombre (minúsculas + sin acentos + guiones), con sufijo `-2`, `-3`… en colisiones. Único en BD.
- **Soft delete**: `Product.Activo = false`. Público filtra por Activo; admin puede listar inactivos con `?incluirInactivos=true`.
- **Guardar en tabla del panel**: patrón borrador → botón "Guardar" explícito por fila (aparece cuando hay cambios). NO autosave onBlur.

### Convenciones vigentes (recordatorio)

- **Capas backend**: Models → DbContext → Services (I + impl registrada en DI) → Controllers. Nada de lógica de negocio en controllers.
- **Estilo**: solo tokens de `styles.css` — `var(--color-*)`, `var(--space-*)`, `var(--radius-*)`. Clases `.card`, `.table`, `.tag`, `.seg`, `.btn`, `.field`, `.input`, `.plate`, `.dialog`. Nada de hex/px/font-family sueltos. Énfasis con itálica, NO negrita.
- **Precios**: `Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 })`. Números con `font-variant-numeric: tabular-nums`.
- **Idioma**: español colombiano en toda la UI y mensajes de error.
- **Cerrar cada módulo punta a punta antes del siguiente**: migración → endpoint REST → pantalla React conectada real → verificación contra BD.
- **Prototipo HTML**: fuente de verdad visual. Copiar disposición y clases, no inventar markup nuevo.

### Trampas conocidas (para no perder tiempo)

- **PowerShell 5.1 + JSON con acentos**: `Invoke-RestMethod -Body $string -ContentType 'application/json'` con "Sáchica", "Sofás", etc. manda UTF-16 y la API devuelve 400. Solución:
  ```powershell
  $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
  Invoke-RestMethod -Uri $uri -Method Post -Body $bytes -ContentType 'application/json; charset=utf-8'
  ```
- **`dotnet ef` global es 8.0.27 pero runtime es 9.0.4**: funciona (avisa por consola). Actualizar con `dotnet tool update --global dotnet-ef` cuando puedas.
- **`app.UseHttpsRedirection()` con URLs solo HTTP**: emite warning "Failed to determine the https port" en cada arranque. No es crítico, se puede sacar en dev.
- **Migración generada después de un build**: si arrancás con `--no-build` inmediatamente después, el DLL viejo no incluye la migración nueva y `MigrateAsync()` no la encuentra. Solución: `dotnet build` primero.

