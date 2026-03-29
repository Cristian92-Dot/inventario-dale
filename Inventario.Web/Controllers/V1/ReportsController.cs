using Inventario.Application.Abstractions.Services;
using Inventario.Application.Common.Exceptions;
using Inventario.Web.Exports;
using Inventario.Web.Exports.Pdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Web.Controllers.V1;

/// <summary>
/// Expone reportes operativos y administrativos desde la API.
/// </summary>
/// <remarks>
/// Estos endpoints están orientados a monitoreo, abastecimiento, seguimiento de ventas y revisión administrativa.
/// Por seguridad, el acceso queda restringido a administradores.
/// </remarks>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOnly")]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, IProductService productService, IWebHostEnvironment environment, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _productService = productService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Devuelve los indicadores principales que alimentan el dashboard administrativo.
    /// </summary>
    /// <remarks>
    /// Resume el estado actual del inventario y las ventas con datos de productos activos, ventas acumuladas, stock bajo y total vendido hoy.
    /// </remarks>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Objeto con métricas listas para mostrarse en el dashboard.</returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await _reportService.GetDashboardAsync(cancellationToken);
        return Ok(Success(result));
    }

    /// <summary>
    /// Devuelve el listado de productos con stock bajo para procesos de abastecimiento.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Productos actualmente marcados como requeridos para reposición.</returns>
    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock(CancellationToken cancellationToken)
    {
        var result = await _productService.GetLowStockAsync(cancellationToken);
        return Ok(Success(result));
    }

    /// <summary>
    /// Devuelve los registros recientes de auditoría funcional.
    /// </summary>
    /// <remarks>
    /// Incluye eventos recientes relacionados con cambios sobre productos, accesos, usuarios y ventas.
    /// Es útil para trazabilidad administrativa y revisión de actividad operativa.
    /// </remarks>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Entradas recientes de auditoría ordenadas desde la más nueva.</returns>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs(CancellationToken cancellationToken)
    {
        var result = await _reportService.GetRecentAuditLogsAsync(cancellationToken);
        return Ok(Success(result));
    }

    /// <summary>
    /// Descarga el catálogo de productos activos en formato CSV.
    /// </summary>
    [HttpGet("products/export")]
    public async Task<IActionResult> ExportProducts(CancellationToken cancellationToken)
    {
        var rows = await _reportService.GetProductExportAsync(cancellationToken);
        return File(CsvExportBuilder.BuildReport(rows), "text/csv", $"productos-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    /// <summary>
    /// Descarga el reporte de stock bajo en formato CSV.
    /// </summary>
    [HttpGet("low-stock/export")]
    public async Task<IActionResult> ExportLowStock(CancellationToken cancellationToken)
    {
        var rows = await _reportService.GetLowStockExportAsync(cancellationToken);
        return File(CsvExportBuilder.BuildReport(rows), "text/csv", $"stock-bajo-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    /// <summary>
    /// Descarga una solicitud de compra en PDF usando los productos con stock bajo.
    /// </summary>
    [HttpGet("purchase-request/export-pdf")]
    public async Task<IActionResult> ExportPurchaseRequestPdf(CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetPurchaseRequestAsync(cancellationToken);
            return File(PurchaseRequestPdfBuilder.Build(report, PdfBranding.ResolveWebRootPath(_environment)), "application/pdf", $"solicitud-compra-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate purchase request PDF.");
            throw new PdfGenerationException("No fue posible generar la solicitud de compras en PDF. Intenta nuevamente en unos segundos. Si el problema continúa, comparte el código de seguimiento con soporte.");
        }
    }

    /// <summary>
    /// Descarga el reporte mensual de ventas en formato PDF.
    /// </summary>
    [HttpGet("monthly-sales/export-pdf")]
    public async Task<IActionResult> ExportMonthlySalesPdf([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetMonthlySalesReportAsync(year, month, cancellationToken);
            return File(MonthlySalesPdfBuilder.Build(report, PdfBranding.ResolveWebRootPath(_environment)), "application/pdf", $"ventas-mensuales-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate monthly sales PDF for year {Year} and month {Month}.", year, month);
            throw new PdfGenerationException("No fue posible generar el reporte mensual en PDF. Intenta nuevamente en unos segundos. Si el problema continúa, comparte el código de seguimiento con soporte.");
        }
    }
}
