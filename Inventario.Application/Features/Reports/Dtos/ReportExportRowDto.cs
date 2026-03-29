namespace Inventario.Application.Features.Reports.Dtos;

public class ReportExportRowDto
{
    public string Nombre { get; init; } = string.Empty;
    public string? ImagenPath { get; init; }
    public decimal Precio { get; init; }
    public int StockActual { get; init; }
    public int StockMinimo { get; init; }
    public string EstadoReposicion { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaActualizacion { get; init; }
}
