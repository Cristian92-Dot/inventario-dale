using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Sales.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Inventario.Web.Controllers.V1;

/// <summary>
/// Gestiona el registro de ventas desde la API.
/// </summary>
/// <remarks>
/// El registro de ventas valida stock disponible, calcula totales y descuenta inventario cuando la operación es correcta.
/// Está preparado para protegerse contra reintentos duplicados mediante idempotencia.
/// </remarks>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEmployee")]
[EnableRateLimiting("sales-policy")]
public class SalesController : BaseApiController
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    /// <summary>
    /// Registra una venta compuesta por uno o varios productos.
    /// </summary>
    /// <remarks>
    /// El servidor valida el stock de manera atómica, calcula el total y registra la venta solo si toda la operación puede completarse.
    /// Si el cliente puede reenviar la solicitud, se recomienda usar el encabezado <c>Idempotency-Key</c>.
    /// </remarks>
    /// <param name="request">Productos y cantidades que formarán parte de la venta.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Resumen de la venta registrada con total y líneas procesadas.</returns>
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterSaleRequest request, CancellationToken cancellationToken)
    {
        var result = await _saleService.RegisterAsync(request, cancellationToken);
        return Ok(Success(result, "Venta registrada correctamente."));
    }
}
