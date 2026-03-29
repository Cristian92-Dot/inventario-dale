using Inventario.Application.Common;
using Inventario.Application.Features.Products.Dtos;

namespace Inventario.Application.Abstractions.Services;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductOptionDto>> GetActiveOptionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductRecommendationDto>> GetRelatedAsync(Guid productId, int take = 4, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductRecommendationDto>> GetTopSellingAsync(Guid? excludedProductId = null, int take = 4, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductDto>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
