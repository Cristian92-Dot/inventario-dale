using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Application.Features.Categories.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Inventario.Web.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventario.Web.Pages.Products;

[Authorize(Roles = "ADMIN")]
public class CreateModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IProductCategoryService _productCategoryService;
    private readonly IProductImageStorage _productImageStorage;

    public CreateModel(IProductService productService, IProductCategoryService productCategoryService, IProductImageStorage productImageStorage)
    {
        _productService = productService;
        _productCategoryService = productCategoryService;
        _productImageStorage = productImageStorage;
    }

    [BindProperty]
    public CreateProductRequest Input { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public List<IFormFile> GalleryFiles { get; set; } = new();

    public IReadOnlyCollection<SelectListItem> CategoryOptions { get; private set; } = Array.Empty<SelectListItem>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            Input.ImagePath = await _productImageStorage.SaveAsync(ImageFile, cancellationToken);
            Input.GalleryImagePaths = await _productImageStorage.SaveManyAsync(GalleryFiles, cancellationToken);
            await _productService.CreateAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Producto creado correctamente.";
            return RedirectToPage("/Products/Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await _productCategoryService.GetActiveOptionsAsync(cancellationToken);
        CategoryOptions = categories.Select(category => new SelectListItem
        {
            Value = category.Name,
            Text = category.Name
        }).ToArray();
    }
}
