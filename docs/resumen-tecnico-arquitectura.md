# Resumen Técnico de Arquitectura

Este documento resume cómo está organizado el proyecto y qué responsabilidad tiene cada carpeta o proyecto principal.

## Solución

Raíz:

- `Inventario.sln`
- `docker-compose.yml`
- `README.md`
- `docs/`

## Proyectos

## `Inventario.Domain`

Responsabilidad:

- representar el núcleo del negocio
- mantener entidades y reglas de dominio sin dependencia de UI o infraestructura

Contenido principal:

- `Entities/`
  - `Product`
  - `Sale`
  - `SaleItem`
  - `RefreshToken`
  - `AuditLog`
  - `ErrorLog`
  - `SecurityEvent`
  - `RequestTrace`
  - `HttpTransactionLog`
  - `IdempotencyRecord`
- `Enums/`
  - roles, acciones de auditoría, severidad, eventos de seguridad
- `Common/`
  - clases base auditables y baja lógica

## `Inventario.Application`

Responsabilidad:

- orquestar casos de uso y contratos de aplicación

Contenido principal:

- `Abstractions/`
  - interfaces de servicios, repositorios y utilidades transversales
- `Features/`
  - `Auth/`
  - `Products/`
  - `Sales/`
  - `Reports/`
  - `Users/`
- `Services/`
  - `ProductService`
  - `SaleService`
  - `ReportService`
- `Validators/`
  - reglas con FluentValidation
- `Common/`
  - `ApiResponse`, paginación, contexto actual

## `Inventario.Infrastructure`

Responsabilidad:

- resolver persistencia, autenticación, repositorios y servicios técnicos

Contenido principal:

- `Persistence/`
  - `ApplicationDbContext`
  - `Migrations/`
- `Repositories/`
  - acceso EF Core a productos, ventas, auditoría, idempotencia
- `Identity/`
  - `ApplicationUser`
  - `ApplicationRole`
- `Services/`
  - `AuthService`
  - `JwtTokenService`
  - `UserManagementService`
  - `CurrentUserService`
  - `DataSeeder`
- `DependencyInjection.cs`
  - registro de servicios de infraestructura

## `Inventario.Web`

Responsabilidad:

- host principal del sistema
- expone API REST y frontend Razor Pages

Contenido principal:

- `Controllers/V1/`
  - `AuthController`
  - `ProductsController`
  - `SalesController`
  - `ReportsController`
  - `UsersController`
- `Pages/`
  - `Account/`
  - `Dashboard/`
  - `Products/`
  - `Sales/`
  - `Reports/`
  - `Users/`
  - `Audit/`
- `Middleware/`
  - `CorrelationIdMiddleware`
  - `RequestTraceMiddleware`
  - `HttpTransactionLoggingMiddleware`
  - `GlobalExceptionMiddleware`
  - `IdempotencyMiddleware`
  - `SecurityHeadersMiddleware`
- `Swagger/`
  - filtros y ejemplos para Swagger
- `Exports/`
  - construcción de archivos CSV
- `wwwroot/css/`
  - estilos globales del backoffice
- `wwwroot/js/`
  - comportamiento global del frontend

## `Inventario.Tests.Unit`

Responsabilidad:

- validar reglas de negocio y servicios de aplicación

Cobertura actual:

- creación de producto
- descuento de stock
- reposición
- validación de stock insuficiente

## `Inventario.Tests.Integration`

Responsabilidad:

- validar flujos reales del sistema con host web y base aislada

Cobertura actual:

- login
- refresh
- logout
- autorización
- ventas
- idempotencia

## Flujo backend resumido

1. Llega el request a `Inventario.Web`
2. Pasa por middleware técnico
3. Llega al controller REST o Razor Page
4. El controller/page usa servicios de `Application`
5. Los servicios consultan repositorios/Identity en `Infrastructure`
6. EF Core persiste en SQL Server
7. Se generan auditoría, trazabilidad y logging técnico

## Flujo frontend resumido

1. El usuario inicia sesión
2. Entra al panel principal
3. Navega por módulos del backoffice
4. Cada página Razor usa servicios del backend ya registrados en DI
5. El layout global aplica tema, navegación, alertas y scripts reutilizables

## Datos técnicos importantes

- base de datos: SQL Server en Docker
- autenticación web: Identity cookie
- autenticación API: JWT Bearer
- trazabilidad: `CorrelationId` + `TraceId`
- auditoría funcional: `AuditLogs`
- errores técnicos: `ErrorLogs`
- seguridad: `SecurityEvents`
- request/response HTTP: `HttpTransactionLogs`
- idempotencia: `IdempotencyRecords`

