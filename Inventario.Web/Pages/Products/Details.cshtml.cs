using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Products.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Products;

[Authorize(Roles = "ADMIN,EMPLEADO")]
public class DetailsModel : PageModel
{
    private readonly IProductService _productService;

    public DetailsModel(IProductService productService)
    {
        _productService = productService;
    }

    public ProductDto? Product { get; private set; }
    public IReadOnlyCollection<ProductRecommendationDto> RelatedProducts { get; private set; } = Array.Empty<ProductRecommendationDto>();
    public IReadOnlyCollection<ProductRecommendationDto> TopSellingProducts { get; private set; } = Array.Empty<ProductRecommendationDto>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Product = await _productService.GetByIdAsync(id, cancellationToken);
        if (Product is null)
        {
            return NotFound();
        }

        RelatedProducts = await _productService.GetRelatedAsync(id, 4, cancellationToken);
        TopSellingProducts = await _productService.GetTopSellingAsync(id, 4, cancellationToken);
        return Page();
    }
}
