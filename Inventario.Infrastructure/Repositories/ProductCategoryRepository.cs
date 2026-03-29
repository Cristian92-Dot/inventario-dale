using Inventario.Application.Abstractions.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductCategoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<ProductCategory>().AddAsync(category, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProductCategory>()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProductCategory>()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProductCategory>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ProductCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProductCategory>()
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default)
    {
        var highest = await _dbContext.Set<ProductCategory>()
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);

        return (highest ?? -1) + 1;
    }
}
