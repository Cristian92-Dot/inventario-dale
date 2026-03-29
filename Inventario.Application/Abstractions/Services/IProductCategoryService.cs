using Inventario.Application.Features.Categories.Dtos;

namespace Inventario.Application.Abstractions.Services;

public interface IProductCategoryService
{
    Task<IReadOnlyCollection<ProductCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductCategoryOptionDto>> GetActiveOptionsAsync(CancellationToken cancellationToken = default);
    Task<ProductCategoryDto> CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken = default);
    Task ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default);
}
