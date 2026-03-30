# Arquitectura C4 de DaLE Inventario

Este documento describe la arquitectura del sistema usando el enfoque C4. La idea es explicar el sistema desde lo más general hasta lo más cercano al código, con un lenguaje claro y útil para personas técnicas y no técnicas.

En esta solución, `DaLE` significa `Dashboard de Logística e Existencias`.

## Diagramas visuales listos para entrega

- `diagramas-c4/contexto.svg`
- `diagramas-c4/contenedores.svg`
- `diagramas-c4/componentes.svg`
- `diagramas-c4/aplicacion.svg`
- `diagramas-c4/infraestructura.svg`
- `diagramas-c4/flujo-venta.svg`
- `diagramas-c4/flujo-pdf.svg`

## 1. Nivel 1 - Contexto

DALE Inventario es un sistema interno para controlar productos, ventas, alertas de stock bajo, usuarios y reportes operativos.

### Actores principales

- `Administrador`: gestiona productos, categorías, usuarios, auditoría y exportaciones.
- `Empleado`: consulta productos, revisa detalle y registra ventas.
- `Responsable operativo`: consume reportes y solicitudes de compra en PDF.

### Sistemas externos o dependencias

- `SQL Server`: almacena datos de negocio, usuarios, trazabilidad y auditoría.
- `Navegador web`: interfaz usada por administradores y empleados.
- `Swagger/OpenAPI`: interfaz técnica para probar la API REST.
- `Docker`: apoyo para levantar SQL Server localmente.

![Diagrama C4 de contexto](diagramas-c4/contexto.svg)

```mermaid
flowchart LR
    Admin[Administrador]
    Employee[Empleado]
    Ops[Responsable operativo]
    Browser[Navegador web]
    Swagger[Swagger UI]
    System[DaLE Inventario]
    Sql[(SQL Server)]
    Docker[Docker / SQL Server local]

    Admin --> Browser --> System
    Employee --> Browser
    Ops --> Browser
    Admin --> Swagger --> System
    System --> Sql
    Docker --> Sql
```

## 2. Nivel 2 - Contenedores

La solución está desplegada como una aplicación ASP.NET Core que combina interfaz web y API, apoyada por una base de datos SQL Server.

### Contenedores

- `Inventario.Web`
  - aloja Razor Pages, API REST, middleware, autenticación y Swagger.
- `SQL Server`
  - guarda productos, ventas, usuarios, refresh tokens, auditoría y trazabilidad.
- `Archivos estáticos`
  - almacena imágenes del catálogo en `wwwroot`.

![Diagrama C4 de contenedores](diagramas-c4/contenedores.svg)

```mermaid
flowchart LR
    User[Usuario interno]
    Web[Inventario.Web<br/>Razor Pages + API REST + Swagger]
    Files[wwwroot / imágenes de productos]
    Db[(SQL Server)]

    User --> Web
    Web --> Db
    Web --> Files
```

## 3. Nivel 3 - Componentes principales

## 3.1 Dentro de `Inventario.Web`

- `Razor Pages`
  - UI para login, dashboard, productos, ventas, categorías, usuarios y reportes.
- `API Controllers`
  - endpoints REST versionados en `/api/v1`.
- `Middleware`
  - correlation id, manejo global de excepciones, seguridad HTTP, idempotencia y logging.
- `Swagger`
  - documentación y pruebas de la API.
- `Exports/Pdf`
  - generación de documentos PDF para reportes operativos.

![Diagrama C4 de componentes](diagramas-c4/componentes.svg)

```mermaid
flowchart TD
    UI[Razor Pages]
    Api[API Controllers]
    Middleware[Middleware HTTP]
    Swagger[Swagger / OpenAPI]
    Pdf[Generación PDF]
    App[Servicios de aplicación]

    UI --> Middleware --> App
    Api --> Middleware --> App
    Swagger --> Api
    UI --> Pdf
    Api --> Pdf
    Pdf --> App
```

## 3.2 Dentro de `Inventario.Application`

- `Services`
  - coordinan reglas de negocio.
- `Validators`
  - aplican reglas de entrada con FluentValidation.
- `DTOs`
  - definen contratos entre capas.
- `Abstractions`
  - interfaces para servicios y repositorios.

![Diagrama de la capa de aplicaci&#243;n](diagramas-c4/aplicacion.svg)

```mermaid
flowchart LR
    Controllers[Web / API]
    Services[Application Services]
    Validators[Validators]
    Repos[IRepository Interfaces]
    Infra[Infrastructure]

    Controllers --> Services
    Services --> Validators
    Services --> Repos
    Repos --> Infra
```

## 3.3 Dentro de `Inventario.Infrastructure`

- `ApplicationDbContext`
  - contexto EF Core.
- `Repositories`
  - acceso a datos de productos, ventas, reportes y categorías.
- `Identity`
  - usuarios, roles y autenticación persistida.
- `DataSeeder`
  - usuarios, categorías, productos demo e imágenes iniciales.

![Diagrama de la capa de infraestructura](diagramas-c4/infraestructura.svg)

```mermaid
flowchart LR
    App[Application Services]
    Repo[Repositories]
    DbContext[ApplicationDbContext]
    Identity[ASP.NET Identity]
    Seed[DataSeeder]
    Sql[(SQL Server)]

    App --> Repo --> DbContext --> Sql
    App --> Identity --> Sql
    Seed --> DbContext
    Seed --> Identity
```

## 4. Nivel 4 - Relación con el código

Las piezas principales del proyecto se distribuyen así:

- `Inventario.Domain`
  - entidades como `Product`, `Sale`, `SaleItem`, `ProductCategory`, `ProductGalleryImage`
- `Inventario.Application`
  - servicios como `ProductService`, `SaleService`, `ReportService`, `ProductCategoryService`
- `Inventario.Infrastructure`
  - `ApplicationDbContext`, repositorios y seeding
- `Inventario.Web`
  - páginas Razor, controladores API, middleware, branding PDF y Swagger

## 5. Flujo principal: registrar una venta

![Diagrama del flujo de venta](diagramas-c4/flujo-venta.svg)

```mermaid
sequenceDiagram
    participant U as Usuario
    participant W as Razor/API
    participant S as SaleService
    participant R as Repositorios
    participant DB as SQL Server

    U->>W: Envía venta con productos y cantidades
    W->>S: Valida y procesa solicitud
    S->>R: Consulta productos y stock
    R->>DB: Lee productos
    DB-->>R: Devuelve datos
    R-->>S: Productos encontrados
    S->>S: Calcula total y descuenta stock
    S->>R: Persiste venta y cambios de stock
    R->>DB: Guarda Sale y SaleItems
    DB-->>R: Confirmación
    R-->>S: Operación completada
    S-->>W: Venta registrada
    W-->>U: Respuesta exitosa
```

## 6. Flujo principal: generar un PDF

![Diagrama del flujo de PDF](diagramas-c4/flujo-pdf.svg)

```mermaid
sequenceDiagram
    participant U as Usuario administrador
    participant W as Razor/API
    participant RS as ReportService
    participant PDF as PDF Builder
    participant FS as Archivos estáticos

    U->>W: Solicita exportación PDF
    W->>RS: Obtiene datos del reporte
    RS-->>W: Datos listos
    W->>PDF: Construye documento
    PDF->>FS: Busca logo e imágenes optimizadas
    FS-->>PDF: Devuelve archivos
    PDF-->>W: PDF generado
    W-->>U: Descarga del archivo
```

## 7. Decisiones técnicas importantes

##  Decisiones de Arquitectura

| Decisión | Motivo |
|----------|--------|
| Separación en capas (`Domain`, `Application`, `Infrastructure`, `Web`) | Permite una organización clara de responsabilidades, facilitando el mantenimiento y la evolución del sistema |
| Uso de Razor Pages para el backoffice | Agiliza el desarrollo de interfaces internas con una estructura simple y productiva |
| Implementación de API REST versionada | Facilita la integración con otros sistemas, pruebas controladas y crecimiento futuro |
| Autenticación con Identity + JWT | Permite diferenciar la seguridad entre la aplicación web y los consumidores de la API |
| Uso de EF Core con SQL Server | Proporciona una integración sólida con .NET y simplifica la gestión de migraciones |
| Almacenamiento de rutas de imágenes en lugar de blobs en base de datos | Reduce la complejidad y mejora el rendimiento al servir archivos |
| Validaciones con FluentValidation | Centraliza las reglas de negocio de entrada con mayor claridad y mantenibilidad |
| Manejo global de excepciones | Garantiza uniformidad en el manejo de errores y mejora la trazabilidad del sistema |

