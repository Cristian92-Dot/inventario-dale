using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Categories.Dtos;
using Inventario.Application.Features.Products.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Inventario.Web.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventario.Web.Pages.Products;

[Authorize(Roles = "ADMIN")]
public class EditModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IProductCategoryService _productCategoryService;
    private readonly IProductImageStorage _productImageStorage;

    public EditModel(IProductService productService, IProductCategoryService productCategoryService, IProductImageStorage productImageStorage)
    {
        _productService = productService;
        _productCategoryService = productCategoryService;
        _productImageStorage = productImageStorage;
    }

    [BindProperty]
    public UpdateProductRequest Input { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public List<IFormFile> GalleryFiles { get; set; } = new();

    [BindProperty]
    public bool RemoveCurrentImage { get; set; }

    [BindProperty]
    public List<Guid> GalleryImageIdsToRemove { get; set; } = new();

    [BindProperty]
    public Dictionary<Guid, int> GallerySortOrders { get; set; } = new();

    public Guid Id { get; set; }
    public IReadOnlyCollection<ProductGalleryImageDto> ExistingGalleryImages { get; private set; } = Array.Empty<ProductGalleryImageDto>();
    public IReadOnlyCollection<SelectListItem> CategoryOptions { get; private set; } = Array.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        Id = id;
        Input = new UpdateProductRequest
        {
            Name = product.Name,
            Category = product.Category,
            Brand = product.Brand,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            MinStock = product.MinStock,
            ImagePath = product.ImagePath,
            GalleryImagePaths = product.GalleryImages.Select(x => x.ImagePath).ToArray()
        };
        ExistingGalleryImages = product.GalleryImages;
        GallerySortOrders = product.GalleryImages.ToDictionary(x => x.Id, x => x.SortOrder);
        await LoadCategoriesAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            Id = id;
            await LoadExistingGalleryAsync(id, cancellationToken);
            return Page();
        }

        try
        {
            var existing = await _productService.GetByIdAsync(id, cancellationToken);
            if (existing is null)
            {
                return NotFound();
            }

            Input.ImagePath = await _productImageStorage.ReplaceAsync(existing.ImagePath, ImageFile, RemoveCurrentImage, cancellationToken);
            var removedGalleryPaths = existing.GalleryImages
                .Where(image => GalleryImageIdsToRemove.Contains(image.Id))
                .Select(image => image.ImagePath)
                .ToArray();

            var keptGalleryPaths = existing.GalleryImages
                .Where(image => !GalleryImageIdsToRemove.Contains(image.Id))
                .OrderBy(image => GallerySortOrders.TryGetValue(image.Id, out var sortOrder) ? sortOrder : image.SortOrder)
                .Select(image => image.ImagePath)
                .ToList();

            var newGalleryPaths = await _productImageStorage.SaveManyAsync(GalleryFiles, cancellationToken);
            _productImageStorage.DeleteMany(removedGalleryPaths);
            Input.GalleryImagePaths = keptGalleryPaths.Concat(newGalleryPaths).ToArray();
            await _productService.UpdateAsync(id, Input, cancellationToken);
            TempData["SuccessMessage"] = "Producto actualizado correctamente.";
            return RedirectToPage("/Products/Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            ModelState.AddModelError(string.Empty, ex.Message);
            Id = id;
            await LoadExistingGalleryAsync(id, cancellationToken);
            return Page();
        }
    }

    private async Task LoadExistingGalleryAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        ExistingGalleryImages = product?.GalleryImages ?? Array.Empty<ProductGalleryImageDto>();
        GallerySortOrders = ExistingGalleryImages.ToDictionary(x => x.Id, x => x.SortOrder);
    }

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await _productCategoryService.GetAllAsync(cancellationToken);
        CategoryOptions = categories
            .Select(category => new SelectListItem
            {
                Value = category.Name,
                Text = category.IsActive ? category.Name : $"{category.Name} (inactiva)",
                Disabled = !category.IsActive && !string.Equals(category.Name, Input.Category, StringComparison.OrdinalIgnoreCase)
            })
            .ToArray();
    }
}
