<p align="right"><a href="README.en.md">🇬🇧 Read in English</a></p>

# Nogal — tienda de muebles + panel de fábrica

Sistema real para una fábrica de muebles en **Puente Aranda, Bogotá** que vende directo al consumidor. Cubre dos productos en un mismo dominio:

- **Tienda pública** — home, catálogo con filtros, ficha con visor AR 3D, carrito, checkout con pasarela Wompi y chat "Nogalito".
- **Panel interno** — login, dashboard de ventas con gráficas por rangos de fechas, CRUD de productos, gestión de pedidos, producción e inventario.

Moneda **COP** (`$ 1.890.000`, es-CO). Idioma: español colombiano.

> Proyecto en producción para el negocio familiar. El repositorio es público como pieza de portafolio — ver [licencia](#licencia) antes de reutilizar cualquier parte.

---

## Preview

<p align="center">
  <img src="docs/screenshots/01-home.png" alt="Home — hero y categorías" width="800"><br>
  <sub>Home pública — "Somos la fábrica, no el intermediario".</sub>
</p>

<table>
  <tr>
    <td><img src="docs/screenshots/02-catalogo.png" alt="Catálogo con filtros" width="400"><br><sub>Catálogo con filtros por categoría, material y precio.</sub></td>
    <td><img src="docs/screenshots/03-ficha.png" alt="Ficha de producto con visor 3D" width="400"><br><sub>Ficha con visor AR 3D (<code>@google/model-viewer</code>).</sub></td>
  </tr>
  <tr>
    <td><img src="docs/screenshots/04-carrito.png" alt="Carrito y checkout" width="400"><br><sub>Carrito con variantes de acabado.</sub></td>
    <td><img src="docs/screenshots/05-wompi.png" alt="Checkout Wompi demo" width="400"><br><sub>Pasarela Wompi en modo demo.</sub></td>
  </tr>
  <tr>
    <td><img src="docs/screenshots/06-login.png" alt="Login del panel" width="400"><br><sub>Login del panel interno.</sub></td>
    <td><img src="docs/screenshots/07-dashboard.png" alt="Dashboard de ventas" width="400"><br><sub>Dashboard con rango de fechas, KPI y mezcla por categoría.</sub></td>
  </tr>
  <tr>
    <td><img src="docs/screenshots/08-pedidos.png" alt="Panel de pedidos" width="400"><br><sub>Pedidos, con estados y cambio de flujo.</sub></td>
    <td><img src="docs/screenshots/09-productos.png" alt="CRUD de productos" width="400"><br><sub>Productos: subida de imagen, edición inline, URLs 3D.</sub></td>
  </tr>
</table>

---

## Stack

**Backend**
- ASP.NET Core **.NET 9** Web API
- Entity Framework Core 9 + **PostgreSQL 16**
- JWT firmado con BCrypt, servido como **cookie `HttpOnly`** (sin token en `localStorage`)
- Swagger / OpenAPI
- Cola en background para generación 3D (`IHostedService`)

**Frontend**
- **React 18 + TypeScript** con Vite
- React Router 6
- `@google/model-viewer` para AR (WebXR, Scene Viewer, Quick Look)
- CSS puro con sistema de tokens propio ("Classical", en `styles.css`)

**Infra**
- `docker compose` — Postgres + API + Nginx sirviendo el build de Vite
- **GitHub Actions**: build .NET, typecheck + build Vite, build de imágenes Docker con caché

---

## Módulos implementados

| Módulo | Estado | Notas |
|---|---|---|
| Catálogo con filtros (categoría, material, precio) | Cerrado | `GET /api/products` con paginación |
| Ficha de producto + variantes de acabado | Cerrado | Precio y stock por variante |
| Carrito y checkout | Cerrado | Estado en servidor por sesión |
| **Pasarela Wompi (modo demo integrado)** | Cerrado | `IPaymentService` con dispatcher Demo/Wompi |
| Visor AR 3D por producto | Cerrado | GLB/GLTF + USDZ, `model-viewer` oficial |
| Generación 3D image-to-3D (cola) | Scaffold | Adaptador Meshy listo, falta credencial real |
| Panel — Resumen (dashboard) | Cerrado | Rango de fechas, granularidad día/semana/mes, mezcla por categoría, top 5 |
| Panel — Pedidos | Cerrado | Cambio de estado, ciclo de pagos cerrado |
| Panel — Productos (CRUD) | Cerrado | Multipart para imágenes, validación de firma binaria |
| Panel — Producción e inventario | Cerrado | Etapas, capacidad, materiales por reponer |
| Home real, contacto y chat Nogalito | Cerrado | Persistencia e historial en BD |
| Auth JWT en cookie `HttpOnly` | Cerrado | Rol `Admin` para endpoints administrativos |

Roadmap pendiente y contexto de negocio en [`prompt-continuar-funcionalidades.md`](prompt-continuar-funcionalidades.md).

---

## Arquitectura

```text
React + TypeScript + Vite   ──►   Controllers (HTTP, DTOs, [Authorize])
                                       │
                                       ▼
                                    IService  (contrato)
                                       │
                                       ▼
                                    Services  (reglas de negocio, validación, mapeo)
                                       │
                                       ▼
                                    EF Core / AppDbContext
                                       │
                                       ▼
                                    PostgreSQL 16
```

- Sin Repository Pattern: EF Core se consume directo desde los `Services`.
- Los DTOs mantienen las entidades EF fuera de la superficie pública de la API.
- Las validaciones de negocio (nombre, precio, categoría, material, estados) viven en los `Services`; los `Controllers` solo traducen a `400`.
- Migraciones se aplican automáticamente al iniciar con `Database.MigrateAsync()`.
- Fechas: el dominio se interpreta en calendario **Bogotá** y se convierte a UTC antes de tocar Postgres.

Detalle completo de pantallas, tokens y contratos en [`design_handoff_nogal/README.md`](design_handoff_nogal/README.md).

---

## Levantar en local

### Opción A — Docker (recomendado)

```powershell
copy .env.example .env
# edita .env: cambia JWT_KEY y POSTGRES_PASSWORD

docker compose up --build
```

- Tienda:   http://localhost
- API:      http://localhost:5199 (Swagger en `/swagger` en Development)
- Postgres: `localhost:5432` (usuario `nogal`)

Los uploads de imágenes y los modelos 3D persisten en el volumen `nogal_api_uploads`.

### Opción B — Sin Docker (dev con hot reload)

Requisitos: **.NET 9 SDK**, **Node 20+**, **Postgres 16** local (o el contenedor anterior levantado solo).

```powershell
# Backend
cd design_handoff_nogal\backend\NogalApi
dotnet restore
dotnet run
# API en http://localhost:5199

# Frontend (otra terminal)
cd design_handoff_nogal\frontend\nogal-web
npm install
npm run dev
# Vite en http://localhost:5173, proxy hacia :5199
```

Credenciales iniciales del panel se siembran desde `AdminSeed` en `appsettings.json`. Cámbialas en cualquier entorno real.

---

## Verificación

```powershell
# Backend
cd design_handoff_nogal\backend\NogalApi
dotnet build .\NogalApi.sln

# Frontend
cd design_handoff_nogal\frontend\nogal-web
npm run build
```

CI corre lo mismo en cada push a `main` y en cada PR (`.github/workflows/ci.yml`).

---

## Estructura

```
definitivo/
├── design_handoff_nogal/
│   ├── backend/NogalApi/       # ASP.NET Core 9 + EF Core + Postgres
│   ├── frontend/nogal-web/     # React 18 + Vite + TypeScript
│   ├── Nogal Muebles.dc.html   # Prototipo de diseño (fuente visual de verdad)
│   ├── styles.css              # Sistema "Classical" (tokens + componentes)
│   ├── sistema-classical.md    # Guía escrita del sistema de diseño
│   └── README.md               # Spec funcional pantalla por pantalla
├── docker-compose.yml
├── .env.example
├── .github/workflows/ci.yml
└── README.md                   # (este archivo)
```

---

## Decisiones técnicas destacadas

- **Cookie `HttpOnly` sobre `localStorage`** para el JWT: mitiga XSS a costa de exigir CORS con `AllowCredentials()`. El backend acepta el token por cookie *o* por header `Authorization` (Swagger sigue funcionando).
- **Sin Repository Pattern**: agregar una capa sobre EF Core que ya es una abstracción de datos es sobreingeniería para este dominio.
- **Wompi vía `IPaymentService` con dispatcher**: la implementación demo permite cerrar pedidos punta a punta sin credenciales reales; cambiar a Wompi productivo es cuestión de flag y llave.
- **Modelos 3D como URL, no binario**: la API solo guarda `Modelo3dUrl` / `ModeloUsdzUrl`; los archivos viven donde tenga sentido (bucket, CDN). Colas y estados listos para conectar un proveedor image-to-3D real (Meshy scaffolded).
- **Fechas en calendario Bogotá**: los rangos del dashboard se interpretan como día de negocio local y se convierten a UTC solo al consultar. Evita reportes que "se corren un día".

---

## Licencia

**Copyright © 2026 Anderson Suarez. All rights reserved.**

Este repositorio es **público como pieza de portafolio** y como despliegue del sistema operativo de una fábrica de muebles familiar. **No es software de código abierto.**

- Puedes **leer, estudiar y compartir el enlace**.
- **No puedes** copiar, redistribuir, publicar, sublicenciar ni usar este código —total o parcialmente— en productos propios o de terceros, comerciales o no, sin autorización escrita del autor.
- Fragmentos evaluativos (una función suelta a modo de referencia técnica, con atribución) están permitidos bajo *fair use* razonable. Clonar el proyecto para levantar una tienda similar, no.

Ver el archivo [`LICENSE`](LICENSE) para el texto completo.

Si te interesa usarlo para tu negocio o adaptarlo bajo licencia comercial: escríbeme.

---

## Autor

**Anderson Suarez** · titusandersonsuarez@gmail.com
