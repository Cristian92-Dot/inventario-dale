namespace Inventario.Application.Features.Reports.Dtos;

public class DashboardInsightsDto
{
    public IReadOnlyCollection<DashboardChartPointDto> VentasUltimosSieteDias { get; init; } = Array.Empty<DashboardChartPointDto>();
    public IReadOnlyCollection<DashboardChartPointDto> EstadoInventario { get; init; } = Array.Empty<DashboardChartPointDto>();
    public IReadOnlyCollection<DashboardChartPointDto> ProductosCriticos { get; init; } = Array.Empty<DashboardChartPointDto>();
}
