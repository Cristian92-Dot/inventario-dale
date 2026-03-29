using Inventario.Application.Abstractions;
using Inventario.Application.Abstractions.Repositories;
using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Application.Features.Reports.Dtos;
using Inventario.Domain.Entities;

namespace Inventario.Application.Services;

public class ReportService : IReportService
{
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReportService(
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        IAuditLogRepository auditLogRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _auditLogRepository = auditLogRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DashboardMetricsDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetPagedAsync(new ProductQueryRequest
        {
            PageNumber = 1,
            PageSize = 1,
            SortBy = "updated_desc"
        }, cancellationToken);
        var lowStock = await _productRepository.GetLowStockAsync(cancellationToken);
        var totalSales = await _saleRepository.GetTotalSalesCountAsync(cancellationToken);
        var totalSoldToday = await _saleRepository.GetTotalSoldTodayAsync(_dateTimeProvider.UtcNow.Date, cancellationToken);

        return new DashboardMetricsDto
        {
            TotalProducts = products.TotalCount,
            TotalSales = totalSales,
            LowStockProducts = lowStock.Count,
            TotalSoldToday = totalSoldToday
        };
    }

    public async Task<DashboardInsightsDto> GetDashboardInsightsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllActiveAsync(cancellationToken);
        var lowStockProducts = products
            .Where(x => x.RequiresRestock)
            .OrderBy(x => x.Stock)
            .Take(5)
            .Select(x => new DashboardChartPointDto
            {
                Label = x.Name,
                Value = x.Stock
            })
            .ToArray();

        var salesTrend = await _saleRepository.GetDailyTotalsAsync(_dateTimeProvider.UtcNow.Date.AddDays(-6), 7, cancellationToken);
        var inventoryDistribution = new[]
        {
            new DashboardChartPointDto { Label = "Normal", Value = products.Count(x => !x.RequiresRestock && x.Stock > 0) },
            new DashboardChartPointDto { Label = "En reposicion", Value = products.Count(x => x.RequiresRestock && x.Stock > 0) },
            new DashboardChartPointDto { Label = "Agotado", Value = products.Count(x => x.Stock == 0) }
        };

        return new DashboardInsightsDto
        {
            VentasUltimosSieteDias = salesTrend.Select(x => new DashboardChartPointDto
            {
                Label = x.Date.ToString("dd/MM"),
                Value = x.Total
            }).ToArray(),
            EstadoInventario = inventoryDistribution,
            ProductosCriticos = lowStockProducts
        };
    }

    public async Task<IReadOnlyCollection<AuditLogDto>> GetRecentAuditLogsAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetRecentAsync(50, cancellationToken);
        return logs.Select(x => new AuditLogDto
        {
            Id = x.Id,
            ActionType = x.ActionType.ToString(),
            EntityName = x.EntityName,
            UserName = x.UserName,
            CorrelationId = x.CorrelationId,
            CreatedAt = x.CreatedAt
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<ReportExportRowDto>> GetLowStockExportAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetLowStockAsync(cancellationToken);
        return products.Select(MapExportRow).ToArray();
    }

    public async Task<IReadOnlyCollection<ReportExportRowDto>> GetProductExportAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllActiveAsync(cancellationToken);
        return products.Select(MapExportRow).ToArray();
    }

    public async Task<PurchaseRequestPdfDto> GetPurchaseRequestAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetLowStockAsync(cancellationToken);
        return new PurchaseRequestPdfDto
        {
            FechaEmision = _dateTimeProvider.UtcNow,
            Correlativo = $"SC-{_dateTimeProvider.UtcNow:yyyyMMddHHmm}",
            Items = products.Select(product => new PurchaseRequestPdfItemDto
            {
                Nombre = product.Name,
                ImagenPath = product.ImagePath,
                StockActual = product.Stock,
                StockMinimo = product.MinStock,
                CantidadSugerida = Math.Max(product.MinStock - product.Stock + 5, 1),
                Estado = product.Stock == 0 ? "Agotado" : "Requiere reposición"
            }).ToArray()
        };
    }

    public async Task<MonthlySalesPdfDto> GetMonthlySalesReportAsync(int? year, int? month, CancellationToken cancellationToken = default)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        var reportYear = year ?? utcNow.Year;
        var reportMonth = month ?? utcNow.Month;
        var byDay = await _saleRepository.GetMonthlyTotalsByDayAsync(reportYear, reportMonth, cancellationToken);
        var topProducts = await _saleRepository.GetTopProductsByMonthAsync(reportYear, reportMonth, 5, cancellationToken);
        var activeProducts = await _productRepository.GetAllActiveAsync(cancellationToken);
        var productImages = activeProducts.ToDictionary(product => product.Name, product => product.ImagePath, StringComparer.OrdinalIgnoreCase);
        var totalSales = byDay.Sum(x => x.SalesCount);
        var totalSold = byDay.Sum(x => x.Total);

        return new MonthlySalesPdfDto
        {
            Mes = new DateTime(reportYear, reportMonth, 1).ToString("MMMM", new System.Globalization.CultureInfo("es-HN")),
            Anio = reportYear,
            TotalVentas = totalSales,
            TotalVendido = totalSold,
            PromedioPorVenta = totalSales == 0 ? 0 : totalSold / totalSales,
            FechaGeneracion = utcNow,
            TotalesPorDia = byDay.Select(x => new MonthlySalesDayDto
            {
                Fecha = x.Date,
                Ventas = x.SalesCount,
                Total = x.Total
            }).ToArray(),
            ProductosMasVendidos = topProducts.Select(x => new MonthlyTopProductDto
            {
                Nombre = x.ProductName,
                ImagenPath = productImages.TryGetValue(x.ProductName, out var imagePath) ? imagePath : null,
                CantidadVendida = x.Quantity,
                TotalVendido = x.Total
            }).ToArray()
        };
    }

    private static ReportExportRowDto MapExportRow(Product product)
    {
        return new ReportExportRowDto
        {
            Nombre = product.Name,
            ImagenPath = product.ImagePath,
            Precio = product.Price,
            StockActual = product.Stock,
            StockMinimo = product.MinStock,
            EstadoReposicion = product.RequiresRestock ? "Requiere reposicion" : "Normal",
            Estado = product.IsActive ? "Activo" : "Inactivo",
            FechaActualizacion = product.UpdatedAt ?? product.CreatedAt
        };
    }
}
