namespace Inventario.Application.Features.Reports.Dtos;

public class MonthlySalesDayDto
{
    public DateTime Fecha { get; init; }
    public int Ventas { get; init; }
    public decimal Total { get; init; }
}
