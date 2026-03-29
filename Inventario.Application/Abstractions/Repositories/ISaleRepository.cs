using Inventario.Domain.Entities;

namespace Inventario.Application.Abstractions.Repositories;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalSoldTodayAsync(DateTime utcDate, CancellationToken cancellationToken = default);
    Task<int> GetTotalSalesCountAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<(DateTime Date, decimal Total)>> GetDailyTotalsAsync(DateTime fromUtcDate, int days, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<(DateTime Date, int SalesCount, decimal Total)>> GetMonthlyTotalsByDayAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<(string ProductName, int Quantity, decimal Total)>> GetTopProductsByMonthAsync(int year, int month, int top, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<(Guid ProductId, int Quantity)>> GetTopSellingProductIdsAsync(int top, Guid? excludedProductId, CancellationToken cancellationToken = default);
}
