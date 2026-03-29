namespace Inventario.Application.Features.Reports.Dtos;

public class PurchaseRequestPdfItemDto
{
    public string Nombre { get; init; } = string.Empty;
    public string? ImagenPath { get; init; }
    public int StockActual { get; init; }
    public int StockMinimo { get; init; }
    public int CantidadSugerida { get; init; }
    public string Estado { get; init; } = string.Empty;
}
