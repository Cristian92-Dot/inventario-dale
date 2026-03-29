namespace Inventario.Application.Features.Reports.Dtos;

public class PurchaseRequestPdfDto
{
    public string Titulo { get; init; } = "Solicitud de compra por reposición";
    public string Empresa { get; init; } = "Inventario Central S. de R.L.";
    public string AreaSolicitante { get; init; } = "Abastecimiento y Operaciones";
    public string Responsable { get; init; } = "Cristian Wilfredo Flores Pacheco";
    public DateTime FechaEmision { get; init; }
    public string Correlativo { get; init; } = string.Empty;
    public IReadOnlyCollection<PurchaseRequestPdfItemDto> Items { get; init; } = Array.Empty<PurchaseRequestPdfItemDto>();
}
