using Inventario.Application.Abstractions.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SaleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _dbContext.Sales.AddAsync(sale, cancellationToken);
    }

    public async Task<int> GetTotalSalesCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sales.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<(DateTime Date, decimal Total)>> GetDailyTotalsAsync(DateTime fromUtcDate, int days, CancellationToken cancellationToken = default)
    {
        var start = fromUtcDate.Date;
        var end = start.AddDays(days);

        var grouped = await _dbContext.Sales
            .Where(x => x.Date >= start && x.Date < end)
            .GroupBy(x => x.Date.Date)
            .Select(group => new
            {
                Date = group.Key,
                Total = group.Sum(x => x.Total)
            })
            .ToListAsync(cancellationToken);

        return Enumerable.Range(0, days)
            .Select(offset => start.AddDays(offset))
            .Select(date =>
            {
                var value = grouped.FirstOrDefault(x => x.Date == date);
                return (date, value?.Total ?? 0m);
            })
            .ToArray();
    }

    public async Task<IReadOnlyCollection<(DateTime Date, int SalesCount, decimal Total)>> GetMonthlyTotalsByDayAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var grouped = await _dbContext.Sales
            .Where(x => x.Date >= start && x.Date < end)
            .GroupBy(x => x.Date.Date)
            .Select(group => new
            {
                Date = group.Key,
                SalesCount = group.Count(),
                Total = group.Sum(x => x.Total)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Date, x.SalesCount, x.Total)).ToArray();
    }

    public async Task<IReadOnlyCollection<(string ProductName, int Quantity, decimal Total)>> GetTopProductsByMonthAsync(int year, int month, int top, CancellationToken cancellationToken = default)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var items = await _dbContext.SaleItems
            .Include(x => x.Sale)
            .Include(x => x.Product)
            .Where(x => x.Sale != null && x.Sale.Date >= start && x.Sale.Date < end)
            .GroupBy(x => x.Product!.Name)
            .Select(group => new
            {
                ProductName = group.Key,
                Quantity = group.Sum(x => x.Quantity),
                Total = group.Sum(x => x.Subtotal)
            })
            .OrderByDescending(x => x.Quantity)
            .ThenByDescending(x => x.Total)
            .Take(top)
            .ToListAsync(cancellationToken);

        return items.Select(x => (x.ProductName, x.Quantity, x.Total)).ToArray();
    }

    public async Task<IReadOnlyCollection<(Guid ProductId, int Quantity)>> GetTopSellingProductIdsAsync(int top, Guid? excludedProductId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SaleItems
            .Include(x => x.Product)
            .Where(x => x.Product != null && x.Product.IsActive);

        if (excludedProductId.HasValue)
        {
            query = query.Where(x => x.ProductId != excludedProductId.Value);
        }

        var items = await query
            .GroupBy(x => x.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(top)
            .ToListAsync(cancellationToken);

        return items.Select(x => (x.ProductId, x.Quantity)).ToArray();
    }

    public async Task<decimal> GetTotalSoldTodayAsync(DateTime utcDate, CancellationToken cancellationToken = default)
    {
        var nextDate = utcDate.Date.AddDays(1);

        return await _dbContext.Sales
            .Where(x => x.Date >= utcDate.Date && x.Date < nextDate)
            .SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;
    }
}
