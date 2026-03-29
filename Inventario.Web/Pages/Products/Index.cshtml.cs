using Inventario.Application.Abstractions.Services;
using Inventario.Application.Common;
using Inventario.Application.Features.Products.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inventario.Web.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Products;

[Authorize(Roles = "ADMIN,EMPLEADO")]
public class IndexModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IProductCategoryService _productCategoryService;
    private readonly IReportService _reportService;

    public IndexModel(IProductService productService, IProductCategoryService productCategoryService, IReportService reportService)
    {
        _productService = productService;
        _productCategoryService = productCategoryService;
        _reportService = reportService;
    }

    [BindProperty(SupportsGet = true)]
    public ProductQueryRequest Query { get; set; } = new();

    public PagedResult<ProductDto> Products { get; private set; } = new();
    public IReadOnlyCollection<SelectListItem> CategoryOptions { get; private set; } = Array.Empty<SelectListItem>();
    public IReadOnlyCollection<SelectListItem> BrandOptions { get; private set; } = Array.Empty<SelectListItem>();
    public IReadOnlyCollection<SelectListItem> StockStatusOptions { get; } =
    [
        new("Todos los estados", string.Empty),
        new("Disponible", "available"),
        new("Stock bajo", "lowstock"),
        new("Agotado", "outofstock")
    ];
    public IReadOnlyCollection<SelectListItem> SortOptions { get; } =
    [
        new("Más recientes", "updated_desc"),
        new("Más antiguos", "updated_asc"),
        new("Nombre A-Z", "name_asc"),
        new("Nombre Z-A", "name_desc"),
        new("Precio menor a mayor", "price_asc"),
        new("Precio mayor a menor", "price_desc"),
        new("Stock menor a mayor", "stock_asc"),
        new("Stock mayor a menor", "stock_desc")
    ];

    public int AvailableCount => Products.Items.Count(x => x.Stock > 0 && !x.RequiresRestock);
    public int LowStockCount => Products.Items.Count(x => x.Stock > 0 && x.RequiresRestock);
    public int OutOfStockCount => Products.Items.Count(x => x.Stock == 0);
    public int VisibleCategoryCount => Products.Items.Select(x => x.Category).Distinct(StringComparer.OrdinalIgnoreCase).Count();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadFilterOptionsAsync(cancellationToken);
        Products = await _productService.GetPagedAsync(Query, cancellationToken);

        if (string.IsNullOrWhiteSpace(Query.SortBy))
        {
            Query.SortBy = "updated_desc";
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("ADMIN"))
        {
            return Forbid();
        }

        await _productService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Producto desactivado correctamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken cancellationToken)
    {
        if (!User.IsInRole("ADMIN"))
        {
            return Forbid();
        }

        var rows = await _reportService.GetProductExportAsync(cancellationToken);
        return File(CsvExportBuilder.BuildReport(rows), "text/csv", $"productos-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public string BuildPageUrl(int pageNumber)
    {
        return BuildUrl(pageNumber, Query.StockStatus);
    }

    public string BuildStockStatusUrl(string? stockStatus)
    {
        return BuildUrl(1, stockStatus);
    }

    public bool HasActiveFilters()
    {
        return !string.IsNullOrWhiteSpace(Query.Search)
            || !string.IsNullOrWhiteSpace(Query.Category)
            || !string.IsNullOrWhiteSpace(Query.Brand)
            || !string.IsNullOrWhiteSpace(Query.StockStatus)
            || !string.IsNullOrWhiteSpace(Query.SortBy) && !string.Equals(Query.SortBy, "updated_desc", StringComparison.OrdinalIgnoreCase);
    }

    private async Task LoadFilterOptionsAsync(CancellationToken cancellationToken)
    {
        var categories = await _productCategoryService.GetActiveOptionsAsync(cancellationToken);
        CategoryOptions = categories.Select(category => new SelectListItem(category.Name, category.Name)).ToArray();

        var brands = await _productService.GetActiveOptionsAsync(cancellationToken);
        BrandOptions = brands
            .Select(product => product.Brand)
            .Where(brand => !string.IsNullOrWhiteSpace(brand))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(brand => brand)
            .Select(brand => new SelectListItem(brand, brand))
            .ToArray();
    }

    private static void AppendParameter(List<string> parameters, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add($"{key}={Uri.EscapeDataString(value)}");
        }
    }

    private string BuildUrl(int pageNumber, string? stockStatus)
    {
        var parameters = new List<string>
        {
            $"Query.PageNumber={pageNumber}",
            $"Query.PageSize={Query.PageSize}"
        };

        AppendParameter(parameters, "Query.Search", Query.Search);
        AppendParameter(parameters, "Query.Category", Query.Category);
        AppendParameter(parameters, "Query.Brand", Query.Brand);
        AppendParameter(parameters, "Query.StockStatus", stockStatus);
        AppendParameter(parameters, "Query.SortBy", Query.SortBy);
        return $"?{string.Join("&", parameters)}";
    }
}
