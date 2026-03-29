using Inventario.Domain.Entities;
using Inventario.Application.Features.Products.Dtos;

namespace Inventario.Application.Abstractions.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<Product?> GetActiveByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetRelatedByCategoryAsync(string category, Guid excludedProductId, int take, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryRequest request, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}
