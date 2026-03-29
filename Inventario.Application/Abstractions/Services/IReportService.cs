using Inventario.Application.Features.Reports.Dtos;

namespace Inventario.Application.Abstractions.Services;

public interface IReportService
{
    Task<DashboardMetricsDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<DashboardInsightsDto> GetDashboardInsightsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AuditLogDto>> GetRecentAuditLogsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReportExportRowDto>> GetProductExportAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReportExportRowDto>> GetLowStockExportAsync(CancellationToken cancellationToken = default);
    Task<PurchaseRequestPdfDto> GetPurchaseRequestAsync(CancellationToken cancellationToken = default);
    Task<MonthlySalesPdfDto> GetMonthlySalesReportAsync(int? year, int? month, CancellationToken cancellationToken = default);
}
