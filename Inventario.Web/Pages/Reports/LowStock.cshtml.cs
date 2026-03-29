using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Web.Exports;
using Inventario.Web.Exports.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Reports;

[Authorize(Roles = "ADMIN,EMPLEADO")]
public class LowStockModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IReportService _reportService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LowStockModel> _logger;

    public LowStockModel(IProductService productService, IReportService reportService, IWebHostEnvironment environment, ILogger<LowStockModel> logger)
    {
        _productService = productService;
        _reportService = reportService;
        _environment = environment;
        _logger = logger;
    }

    public IReadOnlyCollection<ProductDto> Products { get; private set; } = Array.Empty<ProductDto>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Products = await _productService.GetLowStockAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken cancellationToken)
    {
        if (!User.IsInRole("ADMIN"))
        {
            return Forbid();
        }

        var rows = await _reportService.GetLowStockExportAsync(cancellationToken);
        return File(CsvExportBuilder.BuildReport(rows), "text/csv", $"stock-bajo-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public async Task<IActionResult> OnGetExportPdfAsync(CancellationToken cancellationToken)
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
            _logger.LogError(ex, "Failed to generate low stock PDF from report page.");
            TempData["ErrorMessage"] = "No se pudo generar el PDF del reporte de stock bajo. Intenta nuevamente en unos segundos.";
            return RedirectToPage();
        }
    }
}
