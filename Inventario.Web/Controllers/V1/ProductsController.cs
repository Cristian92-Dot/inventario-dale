using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Products.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Web.Controllers.V1;

/// <summary>
/// Expone las operaciones del catálogo de productos de la API.
/// </summary>
/// <remarks>
/// Permite consultar, crear, actualizar y desactivar productos según el rol del usuario autenticado.
/// También expone consultas útiles para stock bajo y listados paginados.
/// </remarks>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEmployee")]
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Devuelve el catálogo de productos activos con filtros y paginación.
    /// </summary>
    /// <remarks>
    /// Puede filtrar por texto, categoría, marca, estado de stock y ordenamiento.
    /// Está pensado para listados operativos, tableros y pantallas de consulta.
    /// </remarks>
    /// <param name="request">Filtros, orden y configuración de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Listado paginado de productos dentro del formato estándar de respuesta.</returns>
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ProductQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.GetPagedAsync(request, cancellationToken);
        return Ok(Success(result));
    }

    /// <summary>
    /// Obtiene el detalle completo de un producto activo.
    /// </summary>
    /// <param name="id">Identificador único del producto.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Detalle del producto o respuesta 404 si no existe o está inactivo.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(Success(result));
    }

    /// <summary>
    /// Lista los productos que requieren reposición por stock bajo.
    /// </summary>
    /// <remarks>
    /// Un producto aparece aquí cuando su stock actual está en nivel crítico respecto al mínimo configurado.
    /// Es útil para abastecimiento, dashboards y seguimiento operativo.
    /// </remarks>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Colección de productos marcados con alerta de stock bajo.</returns>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock(CancellationToken cancellationToken)
    {
        var result = await _productService.GetLowStockAsync(cancellationToken);
        return Ok(Success(result));
    }

    /// <summary>
    /// Registra un nuevo producto dentro del catálogo.
    /// </summary>
    /// <remarks>
    /// Solo los administradores pueden crear productos.
    /// La operación valida nombre único, inventario inicial, categoría activa y datos comerciales del producto.
    /// </remarks>
    /// <param name="request">Datos necesarios para crear el producto.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Producto creado con su identificador y estado actualizado.</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1" }, Success(result, "Producto creado correctamente."));
    }

    /// <summary>
    /// Actualiza la información comercial y de inventario de un producto existente.
    /// </summary>
    /// <remarks>
    /// Recalcula el estado de reposición, conserva trazabilidad y mantiene el producto dentro del flujo auditado.
    /// </remarks>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Datos actualizados del producto.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Producto actualizado.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, request, cancellationToken);
        return Ok(Success(result, "Producto actualizado correctamente."));
    }

    /// <summary>
    /// Desactiva un producto sin eliminarlo físicamente de la base de datos.
    /// </summary>
    /// <remarks>
    /// Esta operación realiza una baja lógica para preservar historial, ventas relacionadas y trazabilidad del negocio.
    /// </remarks>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Respuesta que confirma la desactivación del producto.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return Ok(Success(new { }, "Producto desactivado correctamente."));
    }
}
