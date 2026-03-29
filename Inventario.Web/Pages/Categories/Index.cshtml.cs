using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Categories.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Categories;

[Authorize(Roles = "ADMIN")]
public class IndexModel : PageModel
{
    private readonly IProductCategoryService _productCategoryService;

    public IndexModel(IProductCategoryService productCategoryService)
    {
        _productCategoryService = productCategoryService;
    }

    [BindProperty]
    public CreateProductCategoryRequest Input { get; set; } = new();

    public IReadOnlyCollection<ProductCategoryDto> Categories { get; private set; } = Array.Empty<ProductCategoryDto>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Categories = await _productCategoryService.GetAllAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(cancellationToken);
            return Page();
        }

        try
        {
            await _productCategoryService.CreateAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Categoría registrada correctamente.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(Guid id, CancellationToken cancellationToken)
    {
        await _productCategoryService.ToggleStatusAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Estado de la categoría actualizado correctamente.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Categories = await _productCategoryService.GetAllAsync(cancellationToken);
    }
}
