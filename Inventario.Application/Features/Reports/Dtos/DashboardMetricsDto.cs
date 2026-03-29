namespace Inventario.Application.Features.Reports.Dtos;

/// <summary>
/// Proyeccion compacta del dashboard con metricas operativas clave.
/// </summary>
public class DashboardMetricsDto
{
    /// <summary>
    /// Numero total de productos activos en el catalogo.
    /// </summary>
    public int TotalProducts { get; init; }

    /// <summary>
    /// Numero total de ventas registradas.
    /// </summary>
    public int TotalSales { get; init; }

    /// <summary>
    /// Numero de productos actualmente marcados con stock bajo.
    /// </summary>
    public int LowStockProducts { get; init; }

    /// <summary>
    /// Monto monetario vendido durante el dia actual en UTC.
    /// </summary>
    public decimal TotalSoldToday { get; init; }
}
