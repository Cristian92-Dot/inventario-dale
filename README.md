# Inventario Empresarial

Aplicacion web de inventario y ventas construida con ASP.NET Core 8, Clean Architecture, API REST, Razor Pages, Bootstrap 5 y SQL Server sobre Docker. El objetivo es entregar una prueba tecnica con una base mantenible y cercana a un entorno empresarial real y usando las mejores practicas.

## Caracteristicas principales

- Gestion de productos con alta, consulta, edicion, detalle y baja logica.
- Registro de ventas con multiples productos y descuento automatico de stock.
- Alerta de stock bajo mediante `RequiresRestock` y reporte dedicado.
- Roles `ADMIN` y `EMPLEADO` con restricciones por endpoint y por pagina.
- API REST versionada en `/api/v1` con Swagger y JWT Bearer.
- Refresh token persistido con rotacion y revocacion.
- Auditoria automatica, logs estructurados, `CorrelationId`, `ErrorLog`, `SecurityEvents`, `RequestTraces`, `HttpTransactionLogs` con idempotencia.
- Rate limiting, headers HTTP de seguridad y manejo global de excepciones.
- Pruebas unitarias e integracion ejecutables.
- Se implemento un dashboard con graficas, alertas visuales y exportacion de reportes CSV/PDF.
- Gestion administrativa de usuarios internos con roles y activacion/desactivacion.

## Stack tecnico

- ASP.NET Core 8
- Razor Pages + Bootstrap 5
- ASP.NET Identity + JWT Authentication
- Entity Framework Core 8 + SQL Server
- Serilog
- xUnit + Moq + FluentAssertions
- Docker Compose para SQL Server

## Arquitectura de la solucion

- `Inventario.Domain`
  - entidades, enums y reglas de negocio
- `Inventario.Application`
  - DTOs, contratos, validaciones y servicios de aplicacion
- `Inventario.Infrastructure`
  - EF Core, Identity, JWT, repositorios, seed y persistencia
- `Inventario.Web`
  - API REST, Razor Pages, middleware, Swagger y configuracion HTTP
- `Inventario.Tests.Unit`
  - pruebas unitarias sobre servicios de negocio
- `Inventario.Tests.Integration`
  - pruebas de autenticacion, autorizacion, ventas e idempotencia

## Requisitos previos

- .NET SDK 8
- Docker Desktop
- DBeaver u otro cliente SQL opcional

## Levantar SQL Server con Docker

Primero posicionate en la raiz del repositorio, donde estan `Inventario.sln` y `docker-compose.yml`.

Ejemplo en PowerShell:

```powershell
cd C:\Users\crist\source\repos\Inventario
```

Desde la raiz del repositorio:

```bash
docker compose up -d sqlserver
```

Verificar estado:

```bash
docker compose ps
```

Detener el contenedor cuando ya no lo necesitemos:

```bash
docker compose down
```

## Conexion desde DBeaver

Usa estos datos:

- Host: `localhost`
- Puerto: `1433`
- Base de datos: `InventarioDb`
- Usuario: `sa`
- Password: `InventarioStrongPass123!`
- Propiedad recomendada: `TrustServerCertificate=True`

## Ejecucion de la aplicacion

Se deben mantener posicionado en la raiz del repositorio segun sea la ubicacion que lo coloquemos:

```powershell
cd C:\Users\crist\source\repos\Inventario
```

```bash
dotnet restore Inventario.sln
dotnet build Inventario.sln
dotnet run --project Inventario.Web/Inventario.Web.csproj
```

La aplicacion aplica migraciones y seed automaticamente al iniciar.

URLs locales comunes:

- UI/API: `http://localhost:5150`
- HTTPS: `https://localhost:7049`
- Swagger: `https://localhost:7049/swagger`

## Levantamiento rapido paso a paso

### Terminal 1: base de datos

```powershell
cd C:\Users\crist\source\repos\Inventario
docker compose up -d sqlserver
docker compose ps
```

### Terminal 2: aplicacion web

```powershell
cd C:\Users\crist\source\repos\Inventario
dotnet restore Inventario.sln
dotnet build Inventario.sln
dotnet run --project Inventario.Web/Inventario.Web.csproj
```

### Abrir en navegador

- UI: `http://localhost:5150`
- Swagger: `http://localhost:5150/swagger`

### Para detener todo

Si la app esta corriendo en consola, presiona `Ctrl + C`.

Luego, desde la raiz del repositorio:

```powershell
cd C:\Users\crist\source\repos\Inventario
docker compose down
```

## Usuarios semilla
usuario   / Contraseña
- `admin` / `Admin123*`
- `empleado` / `Empleado123*`

## Datos demo iniciales

Al arrancar por primera vez, el sistema crea productos de ejemplo para que se pueda probar la UI y las ventas sin cargar datos manualmente. Incluye productos normales y productos con stock bajo para validar alertas y reportes.

## Flujo funcional

### UI

- Login en `Razor Pages`
- Panel principal con metricas operativas y graficas
- Productos: listado, crear, editar, detalle y desactivar
- Ventas: formulario dinamico con multiples productos
- Reporte de stock bajo
- Descarga de solicitud de compras en PDF y reporte mensual de ventas en PDF
- Auditoria basica visible para `ADMIN`
- Gestion de usuarios visible para `ADMIN`

### API

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `GET /api/v1/products`
- `GET /api/v1/products/{id}`
- `POST /api/v1/products`
- `PUT /api/v1/products/{id}`
- `DELETE /api/v1/products/{id}`
- `GET /api/v1/products/low-stock`
- `POST /api/v1/sales`
- `GET /api/v1/reports/dashboard`
- `GET /api/v1/reports/low-stock`
- `GET /api/v1/reports/audit-logs`
- `GET /api/v1/reports/products/export`
- `GET /api/v1/reports/low-stock/export`
- `GET /api/v1/reports/purchase-request/export-pdf`
- `GET /api/v1/reports/monthly-sales/export-pdf`
- `GET /api/v1/users`
- `POST /api/v1/users`
- `PUT /api/v1/users/{id}/role`
- `PATCH /api/v1/users/{id}/status`
- `GET /health/live`
- `GET /health/ready`

## Flujo de autenticacion

### API

1. El cliente llama `POST /api/v1/auth/login` con usuario y password.
2. La API devuelve `accessToken` y `refreshToken`.
3. El `accessToken` se envia como `Authorization: Bearer <token>`.
4. Cuando expira, el cliente llama `POST /api/v1/auth/refresh` con el refresh token.
5. La API rota el refresh token y revoca el anterior.
6. `POST /api/v1/auth/logout` revoca el refresh token vigente.

### UI

- La navegacion usa cookies de Identity para la experiencia web.
- La API usa JWT, separando sesion web de autenticacion REST.

## Idempotencia

La API soporta `Idempotency-Key` en endpoints sensibles como:

- `POST /api/v1/sales`
- `POST /api/v1/auth/refresh`

Comportamiento:

- misma llave + mismo payload => replay de la respuesta original
- misma llave + payload distinto => `409 Conflict`
- la respuesta se persiste junto con el hash del request y el endpoint

## Seguridad implementada

- ASP.NET Identity con bloqueo temporal por intentos fallidos
- roles `ADMIN` y `EMPLEADO`
- JWT Bearer para API
- refresh token persistido y rotado
- antiforgery para formularios Razor
- headers HTTP de seguridad:
  - `Content-Security-Policy`
  - `X-Content-Type-Options`
  - `X-Frame-Options`
  - `Referrer-Policy`
  - `Permissions-Policy`
- CORS configurable por entorno mediante `Cors:AllowedOrigins`
- limite de tamano de request en Kestrel
- rate limiting global y por endpoints sensibles
- sin redireccion HTML en respuestas API `401/403`

## Observabilidad y trazabilidad

- `CorrelationId` por request mediante `X-Correlation-Id`
- logging estructurado con Serilog a consola y archivo
- auditoria automatica sobre cambios persistidos
- `ErrorLog` para excepciones relevantes
- `AuditLog` para login, logout, cambios de entidad y eventos de negocio
- `SecurityEvents` para eventos de autenticacion, acceso denegado, rate limiting y fallos sensibles
- `RequestTraces` para trazabilidad tecnica por request con usuario, IP, duracion y estado HTTP
- `HttpTransactionLogs` para request/response completo con enmascarado de campos sensibles

## Reglas de negocio relevantes

- no se permite vender sin stock suficiente
- el total de venta se calcula en servidor
- la venta descuenta stock automaticamente
- un producto pasa a `RequiresRestock` cuando `Stock <= MinStock`
- los productos se desactivan con baja logica
- se usa `RowVersion` para concurrencia optimista en productos

## Pruebas

### Unitarias

```bash
dotnet test Inventario.Tests.Unit/Inventario.Tests.Unit.csproj
```

Cobertura actual:

- producto queda marcado para reposicion
- venta falla por stock insuficiente
- venta valida descuenta stock y recalcula total

### Integracion

```bash
dotnet test Inventario.Tests.Integration/Inventario.Tests.Integration.csproj
```

Cobertura actual:

- login exitoso
- endpoint protegido sin token devuelve `401`
- registrar venta persiste y descuenta stock
- idempotencia en ventas hace replay correcto
- refresh token rota e invalida el anterior
- logout revoca refresh token y bloquea reutilizacion

## Como probar el sistema paso a paso

### 1. Levanta la base de datos

```bash
docker compose up -d sqlserver
```

Opcionalmente valida el contenedor:

```bash
docker compose ps
```

### 2. Inicia la aplicacion

```bash
dotnet run --project Inventario.Web/Inventario.Web.csproj
```

### 3. Abrir la UI

- Navega a `https://localhost:7049` o `http://localhost:5150`
- Podran visualizar la portada del sistema y acceso al login

### 4. Pruebas como administrador

- Ir a `Login`
- Usar `admin` / `Admin123*`
- Revisar el `Dashboard`
- Entrar a `Productos`
- Verificar que ya existan productos demo
- Crear un producto nuevo desde `Nuevo producto`
- Editar uno existente
- Desactivar un producto y validar que desaparezca del listado activo
- Entrar a `Auditoria` y validar que se registran los eventos

### 5. Pruebas  de reporte de stock bajo

- Ir a `Stock bajo`
- Deberian de poder ver productos demo como `Scanner Industrial` o `Lector de Codigos`
- Luego confirmar que el dashboard muestre el conteo de alertas

### 6. Pruebas del  flujo de ventas en UI

- Cerrar sesion
- Iniciar con `empleado` / `Empleado123*`
- Ir a `Ventas`
- Agregar uno o muchos productos
- Registrar una venta
- Regresar a `Productos` o `Stock bajo` y valida el cambio de stock

### 7. Pruebas en  Swagger y JWT

- Abrir `https://localhost:7049/swagger`
- Ejecutar `POST /api/v1/auth/login` con `admin` / `Admin123*`
- Copiar el `accessToken`
- Usar el boton `Authorize` con `Bearer <token>`
- Probar `GET /api/v1/products`
- Probar `GET /api/v1/reports/dashboard`

### 8. Pruebas refresh token

- Desde Swagger ejecutar `POST /api/v1/auth/login`
- Copiar el `refreshToken`
- Llamar `POST /api/v1/auth/refresh`
- Verificar que devuelve un nuevo `refreshToken`
- Intentar reutilizar el refresh token viejo y valida que falle con `401`

### 9. Pruebas idempotencia en ventas

- Obténer un `accessToken` de `empleado`
- En Swagger autoriza con Bearer JWT
- Llamar `POST /api/v1/sales`
- Agregar a un  header `Idempotency-Key` con un GUID cualquiera
- Repetir exactamente la misma solicitud con la misma llave
- Debe devolver la misma respuesta previa y no duplicar la venta
- Si reutilizamos  la misma llave con payload diferente, debe responder `409`

### 10. Pruebas desde DBeaver

- Conectarnos  a `InventarioDb`
- Revisa tablas como:
  - `Products`
  - `Sales`
  - `SaleItems`
  - `RefreshTokens`
  - `AuditLogs`
  - `ErrorLogs`
  - `IdempotencyRecords`
  - `SecurityEvents`
  - `RequestTraces`
  - `HttpTransactionLogs`
- Verifica que los cambios de UI/API se reflejen en la base


## Comandos utiles

Compilar y correr todo lo importante:

```bash
dotnet build Inventario.sln
dotnet test Inventario.Tests.Unit/Inventario.Tests.Unit.csproj
dotnet test Inventario.Tests.Integration/Inventario.Tests.Integration.csproj
```

Crear una nueva migracion:

```bash
dotnet ef migrations add NombreMigracion --project Inventario.Infrastructure/Inventario.Infrastructure.csproj --startup-project Inventario.Web/Inventario.Web.csproj --output-dir Persistence/Migrations
```

## Documentacion adicional

- Resumen tecnico por carpetas: `docs/RESUMEN_TECNICO_ARQUITECTURA.md`

## Decisiones tecnicas clave

- un solo host `Web` para UI y API, pero con capas separadas
- SQL Server desacoplado en Docker para evitar instalaciones locales pesadas
- Razor Pages para acelerar el backoffice sin sacrificar estructura
- Identity para usuarios/roles web y JWT para API
- middleware propio para `CorrelationId`, errores globales, headers con idempotencia

