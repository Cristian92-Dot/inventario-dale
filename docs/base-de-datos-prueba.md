# Base de datos de prueba

Este documento explica los scripts SQL preparados para la revision tecnica del proyecto `Inventario`.

## Objetivo

Se dejaron dos archivos para facilitar la creacion manual de la base de datos sin depender de ejecutar la aplicacion:

- `docs/inventario-db-ddl.sql`
  - crea la base `InventarioDb`
  - crea todas las tablas, llaves primarias, foraneas e indices actuales del sistema
- `docs/inventario-db-dml-prueba.sql`
  - inserta datos de prueba para poder revisar la solucion con informacion ya cargada

## Motor de base de datos

La solucion usa `SQL Server`.

Valores usados en el proyecto:

- servidor: `localhost,1433`
- base de datos: `InventarioDb`
- usuario: `sa`
- password: `InventarioStrongPass123!`

Referencia de configuracion:

- `Inventario.Web/appsettings.Development.json`
- `docker-compose.yml`

## Orden de ejecucion

Ejecutar primero el script de estructura y despues el de datos:

1. `docs/inventario-db-ddl.sql`
2. `docs/inventario-db-dml-prueba.sql`

## Que incluye el DDL

El archivo `docs/inventario-db-ddl.sql` fue generado a partir del modelo actual de `Entity Framework Core`, por lo que representa la estructura real usada por la solucion.

Tablas principales incluidas:

- `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` para autenticacion y autorizacion
- `Products`, `ProductCategories`, `ProductGalleryImages` para el catalogo
- `Sales`, `SaleItems` para el modulo de ventas
- `RefreshTokens` para renovacion de sesion JWT
- `AuditLogs`, `ErrorLogs`, `SecurityEvents`, `RequestTraces`, `HttpTransactionLogs`, `IdempotencyRecords` para auditoria, trazabilidad y seguridad

## Que incluye el DML

El archivo `docs/inventario-db-dml-prueba.sql` inserta informacion base para poder probar flujos funcionales y mostrar evidencia al revisor.

Incluye:

- roles: `ADMIN` y `EMPLEADO`
- usuarios de prueba
- categorias del catalogo
- productos demo con stock normal y stock bajo
- imagen principal y galeria por producto
- ventas de ejemplo con sus partidas
- registros de auditoria, seguridad, request trace, http transaction log e idempotencia

## Usuarios de prueba

- `admin` / `Admin123*`
- `empleado` / `Empleado123*`

## Observaciones importantes

- El DML esta escrito de forma idempotente en los inserts principales usando `IF NOT EXISTS`, para evitar duplicados al ejecutarlo mas de una vez.
- El campo `RowVersion` de `Products` no se inserta manualmente porque SQL Server lo genera automaticamente.
- El esquema contiene tablas de `ASP.NET Identity`, por eso existen tablas como `AspNetUsers`, `AspNetRoles` y sus relaciones.
- Aunque la aplicacion puede sembrar datos automaticamente al iniciar, estos scripts se dejaron para que la base pueda entregarse y revisarse de forma directa.

## Forma rapida de uso



1. conectarse al SQL Server local
2. abrir `docs/inventario-db-ddl.sql` y ejecutarlo completo
3. abrir `docs/inventario-db-dml-prueba.sql` y ejecutarlo completo
4. validar que ya existan datos en `Products`, `Sales` y `AspNetUsers`

## Relacion con la solucion

Los scripts corresponden a la solucion actual:

- `Inventario.Infrastructure/Persistence/ApplicationDbContext.cs`
- `Inventario.Infrastructure/Services/DataSeeder.cs`

