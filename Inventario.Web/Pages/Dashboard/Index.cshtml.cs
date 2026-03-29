using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Application.Features.Reports.Dtos;
using Inventario.Web.Exports.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Dashboard;

[Authorize(Roles = "ADMIN,EMPLEADO")]
public class IndexModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IReportService reportService, IProductService productService, IWebHostEnvironment environment, ILogger<IndexModel> logger)
    {
        _reportService = reportService;
        _productService = productService;
        _environment = environment;
        _logger = logger;
    }

    public DashboardMetricsDto Metrics { get; private set; } = new();
    public DashboardInsightsDto Insights { get; private set; } = new();
    public IReadOnlyCollection<ProductDto> LowStockProducts { get; private set; } = Array.Empty<ProductDto>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Metrics = await _reportService.GetDashboardAsync(cancellationToken);
        Insights = await _reportService.GetDashboardInsightsAsync(cancellationToken);
        LowStockProducts = await _productService.GetLowStockAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetExportProductsAsync(CancellationToken cancellationToken)
    {
        if (!User.IsInRole("ADMIN"))
        {
            return Forbid();
        }

        var rows = await _reportService.GetProductExportAsync(cancellationToken);
        return File(Exports.CsvExportBuilder.BuildReport(rows), "text/csv", $"productos-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public async Task<IActionResult> OnGetExportLowStockAsync(CancellationToken cancellationToken)
    {
        if (!User.IsInRole("ADMIN"))
        {
            return Forbid();
        }

        var rows = await _reportService.GetLowStockExportAsync(cancellationToken);
        return File(Exports.CsvExportBuilder.BuildReport(rows), "text/csv", $"stock-bajo-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public async Task<IActionResult> OnGetExportPurchaseRequestPdfAsync(CancellationToken cancellationToken)
    {
        if (!User.IsInRole("ADMIN"))
        {
            return Forbid();
        }

        try
        {
            var report = await _reportService.GetPurchaseRequestAsync(cancellationToken);
            return File(PurchaseRequestPdfBuilder.Build(report, PdfBranding.ResolveWebRootPath(_environment)), "application/pdf", $"solicitud-compra-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate purchase request PDF from dashboard.");
            TempData["ErrorMessage"] = "No se pudo generar la solicitud de compras en PDF. Intenta nuevamente en unos segundos.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnGetExportMonthlySalesPdfAsync(int? year, int? month, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("ADMIN"))
        {
            return Forbid();
        }

        try
        {
            var report = await _reportService.GetMonthlySalesReportAsync(year, month, cancellationToken);
            return File(MonthlySalesPdfBuilder.Build(report, PdfBranding.ResolveWebRootPath(_environment)), "application/pdf", $"ventas-mensuales-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate monthly sales PDF from dashboard.");
            TempData["ErrorMessage"] = "No se pudo generar el reporte mensual en PDF. Intenta nuevamente en unos segundos.";
            return RedirectToPage();
        }
    }
}
