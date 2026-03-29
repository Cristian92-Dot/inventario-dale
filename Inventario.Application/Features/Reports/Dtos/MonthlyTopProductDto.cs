namespace Inventario.Application.Features.Reports.Dtos;

public class MonthlyTopProductDto
{
    public string Nombre { get; init; } = string.Empty;
    public string? ImagenPath { get; init; }
    public int CantidadVendida { get; init; }
    public decimal TotalVendido { get; init; }
}
