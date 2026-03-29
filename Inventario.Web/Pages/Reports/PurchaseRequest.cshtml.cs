using System.ComponentModel.DataAnnotations;
using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Reports.Dtos;
using Inventario.Web.Exports.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Reports;

[Authorize(Roles = "ADMIN")]
public class PurchaseRequestModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PurchaseRequestModel> _logger;

    public PurchaseRequestModel(IReportService reportService, IWebHostEnvironment environment, ILogger<PurchaseRequestModel> logger)
    {
        _reportService = reportService;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public PurchaseRequestForm Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        return Page();
    }

    public IActionResult OnPostGeneratePdf()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!Input.Items.Any(item => item.CantidadAComprar > 0))
        {
            ModelState.AddModelError(string.Empty, "Debes indicar al menos una cantidad mayor que cero para generar la solicitud.");
            return Page();
        }

        try
        {
            var report = new PurchaseRequestPdfDto
            {
                FechaEmision = Input.FechaEmision,
                Correlativo = Input.Correlativo,
                Empresa = Input.Empresa,
                AreaSolicitante = Input.AreaSolicitante,
                Responsable = Input.Responsable,
                Items = Input.Items
                    .Where(item => item.CantidadAComprar > 0)
                    .Select(item => new PurchaseRequestPdfItemDto
                    {
                        Nombre = item.Nombre,
                        ImagenPath = item.ImagenPath,
                        StockActual = item.StockActual,
                        StockMinimo = item.StockMinimo,
                        CantidadSugerida = item.CantidadAComprar,
                        Estado = item.Estado
                    })
                    .ToArray()
            };

            return File(PurchaseRequestPdfBuilder.Build(report, PdfBranding.ResolveWebRootPath(_environment)), "application/pdf", $"solicitud-compra-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate purchase request PDF from editable page.");
            TempData["ErrorMessage"] = "No se pudo generar la solicitud de compras en PDF. Intenta nuevamente en unos segundos.";
            return RedirectToPage();
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetPurchaseRequestAsync(cancellationToken);
        Input = new PurchaseRequestForm
        {
            Empresa = report.Empresa,
            AreaSolicitante = report.AreaSolicitante,
            Responsable = report.Responsable,
            FechaEmision = report.FechaEmision,
            Correlativo = report.Correlativo,
            Items = report.Items.Select(item => new PurchaseRequestItemForm
            {
                Nombre = item.Nombre,
                ImagenPath = item.ImagenPath,
                StockActual = item.StockActual,
                StockMinimo = item.StockMinimo,
                CantidadAComprar = item.CantidadSugerida,
                Estado = item.Estado
            }).ToList()
        };
    }

    public class PurchaseRequestForm
    {
        public string Empresa { get; set; } = string.Empty;
        public string AreaSolicitante { get; set; } = string.Empty;
        public string Responsable { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string Correlativo { get; set; } = string.Empty;
        public List<PurchaseRequestItemForm> Items { get; set; } = new();
    }

    public class PurchaseRequestItemForm
    {
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenPath { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string Estado { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "La cantidad a comprar no puede ser negativa.")]
        public int CantidadAComprar { get; set; }
    }
}
