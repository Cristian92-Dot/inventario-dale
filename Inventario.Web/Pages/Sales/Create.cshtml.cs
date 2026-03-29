using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Application.Features.Sales.Dtos;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventario.Web.Pages.Sales;

[Authorize(Roles = "ADMIN,EMPLEADO")]
public class CreateModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ISaleService _saleService;

    public CreateModel(IProductService productService, ISaleService saleService)
    {
        _productService = productService;
        _saleService = saleService;
    }

    [BindProperty]
    public RegisterSaleForm Input { get; set; } = new();

    public List<SelectListItem> ProductOptions { get; private set; } = new();
    public IReadOnlyCollection<ProductOptionDto> ProductCatalog { get; private set; } = Array.Empty<ProductOptionDto>();
    public string? PageErrorMessage { get; private set; }
    public string? PageSuccessMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadProductsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadProductsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var sale = await _saleService.RegisterAsync(new RegisterSaleRequest
            {
                Items = Input.Items.Select(x => new RegisterSaleItemRequest
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }).ToArray()
            }, cancellationToken);

            var lowStockProducts = await _productService.GetLowStockAsync(cancellationToken);
            var affectedLowStock = lowStockProducts.Where(product => sale.Items.Any(item => item.ProductId == product.Id)).Select(x => x.Name).ToArray();

            TempData["SuccessMessage"] = affectedLowStock.Any()
                ? $"Venta registrada correctamente. Los productos {string.Join(", ", affectedLowStock)} quedaron en nivel de reposición."
                : "Venta registrada correctamente. El inventario fue actualizado.";

            return RedirectToPage("/Dashboard/Index");
        }
        catch (Exception ex)
        {
            PageErrorMessage = ex.Message;
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }

    private async Task LoadProductsAsync(CancellationToken cancellationToken)
    {
        ProductCatalog = await _productService.GetActiveOptionsAsync(cancellationToken);
        ProductOptions = ProductCatalog.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Stock == 0
                ? $"{x.Name} | Agotado"
                : $"{x.Name} | Stock: {x.Stock} | {x.Price:C}",
            Disabled = x.Stock == 0
        }).ToList();

        if (Input.Items.Count == 0)
        {
            Input.Items.Add(new RegisterSaleItemForm());
        }
    }

    public class RegisterSaleForm
    {
        [Required]
        [MinLength(1, ErrorMessage = "Debes agregar al menos un producto a la venta.")]
        public List<RegisterSaleItemForm> Items { get; set; } = new() { new RegisterSaleItemForm() };
    }

    public class RegisterSaleItemForm
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Quantity { get; set; } = 1;
    }
}
