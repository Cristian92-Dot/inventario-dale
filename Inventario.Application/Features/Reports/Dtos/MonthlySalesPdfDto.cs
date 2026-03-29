namespace Inventario.Application.Features.Reports.Dtos;

public class MonthlySalesPdfDto
{
    public string Empresa { get; init; } = "Inventario Central S. de R.L.";
    public string AreaSolicitante { get; init; } = "Dirección Comercial y Operaciones";
    public string Responsable { get; init; } = "Cristian Wilfredo Flores Pacheco";
    public string Mes { get; init; } = string.Empty;
    public int Anio { get; init; }
    public decimal TotalVendido { get; init; }
    public int TotalVentas { get; init; }
    public decimal PromedioPorVenta { get; init; }
    public IReadOnlyCollection<MonthlySalesDayDto> TotalesPorDia { get; init; } = Array.Empty<MonthlySalesDayDto>();
    public IReadOnlyCollection<MonthlyTopProductDto> ProductosMasVendidos { get; init; } = Array.Empty<MonthlyTopProductDto>();
    public DateTime FechaGeneracion { get; init; }
}
