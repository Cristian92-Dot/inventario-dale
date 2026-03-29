using Inventario.Application.Abstractions.Repositories;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.GalleryImages)
            .Where(x => x.IsActive && ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.GalleryImages)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetActiveByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(x => x.IsActive && x.Name == name, cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.GalleryImages)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.GalleryImages)
            .Where(x => x.IsActive && x.RequiresRestock)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetRelatedByCategoryAsync(string category, Guid excludedProductId, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.GalleryImages)
            .Where(x => x.IsActive && x.Id != excludedProductId && x.Category == category)
            .OrderByDescending(x => x.Stock)
            .ThenBy(x => x.Name)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.Category.Contains(search) ||
                x.Brand.Contains(search) ||
                (x.ShortDescription != null && x.ShortDescription.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();
            query = query.Where(x => x.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            var brand = request.Brand.Trim();
            query = query.Where(x => x.Brand == brand);
        }

        query = request.StockStatus?.Trim().ToLowerInvariant() switch
        {
            "available" => query.Where(x => x.Stock > 0 && !x.RequiresRestock),
            "lowstock" => query.Where(x => x.Stock > 0 && x.RequiresRestock),
            "outofstock" => query.Where(x => x.Stock == 0),
            _ => query
        };

        query = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "name_desc" => query.OrderByDescending(x => x.Name),
            "price_asc" => query.OrderBy(x => x.Price).ThenBy(x => x.Name),
            "price_desc" => query.OrderByDescending(x => x.Price).ThenBy(x => x.Name),
            "stock_asc" => query.OrderBy(x => x.Stock).ThenBy(x => x.Name),
            "stock_desc" => query.OrderByDescending(x => x.Stock).ThenBy(x => x.Name),
            "updated_asc" => query.OrderBy(x => x.UpdatedAt ?? x.CreatedAt).ThenBy(x => x.Name),
            _ => query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ThenBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(x => x.GalleryImages)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
