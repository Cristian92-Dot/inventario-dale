using Inventario.Domain.Entities;

namespace Inventario.Application.Abstractions.Repositories;

public interface IProductCategoryRepository
{
    Task<IReadOnlyCollection<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductCategory>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ProductCategory category, CancellationToken cancellationToken = default);
}
