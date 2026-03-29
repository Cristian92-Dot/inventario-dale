# Changelog

## 2026-03-29

### Base funcional
- Se implementó la gestión de productos con alta, edición, listado y detalle.
- Se implementó el registro de ventas con descuento automático de stock.
- Se agregó la detección de productos con stock bajo y reporte asociado.

### Seguridad y acceso
- Se configuró autenticación con JWT para API y cookies para la interfaz web.
- Se reforzó autorización por roles `ADMIN` y `EMPLEADO` en páginas, handlers y endpoints.
- Se corrigió el acceso por rutas conocidas para evitar ingreso no autorizado a pantallas administrativas.

### Arquitectura y calidad
- Se consolidó la separación por capas: `Domain`, `Application`, `Infrastructure` y `Web`.
- Se agregaron validaciones con DataAnnotations y FluentValidation.
- Se mejoró el manejo global de excepciones con respuestas más claras.
- Se incorporaron excepciones de negocio tipadas para conflictos, no encontrados, permisos y errores PDF.

### Catálogo y experiencia de producto
- Se agregó soporte para imagen principal y galería de productos.
- Se incorporaron categoría, marca, descripción corta y descripción larga.
- Se mejoró la vista detalle con una experiencia más rica, zoom, relacionados y más vendidos.
- Se agregaron filtros avanzados por categoría, marca, estado y ordenamiento.

### Branding y reportes
- Se actualizó el login con la marca `DALE` y una presentación más limpia.
- Se mantuvo el branding del dashboard con `logo-inventario2`.
- Se actualizó el branding PDF para solicitud de compra y ventas mensuales.
- Se incorporaron miniaturas de producto en PDFs y ruta de imagen en exportaciones CSV.
- Se agregó una pantalla previa para ajustar cantidades antes de generar la solicitud de compra en PDF.

### Categorías y administración
- Se agregó catálogo maestro de categorías.
- Se habilitó la administración de categorías desde la interfaz.
- Se amplió el seed inicial con categorías, productos demo e imágenes del catálogo.
- Se agregó edición de usuarios con nombre visible y foto de perfil.

### Documentación
- Se reescribió `README.md` con instrucciones de ejecución, usuarios de prueba y decisiones técnicas.
- Se creó la carpeta `DOCS` con manual de usuario, documentación C4, versión ejecutiva.
- Se agregaron diagramas C4 en `SVG` y `PNG`.


### Configuración por entorno
- Se agregaron `Inventario.Web/appsettings.Staging.json` y `Inventario.Web/appsettings.Production.json` como plantillas seguras.
- Se dejó Swagger activo en `Development` y `Staging`, y desactivado en `Production` por defecto.

### Pruebas
- Se agregaron pruebas unitarias para servicios.
- Se agregaron pruebas de integración para permisos y generación de PDFs.
- Se verificó compilación y ejecución de pruebas al cierre de la entrega.
