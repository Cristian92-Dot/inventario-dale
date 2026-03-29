namespace Inventario.Application.Features.Sales.Dtos;

/// <summary>
/// Payload utilizado para registrar una venta con una o varias lineas.
/// </summary>
public class RegisterSaleRequest
{
    /// <summary>
    /// Coleccion de lineas de producto que componen la venta.
    /// </summary>
    public IReadOnlyCollection<RegisterSaleItemRequest> Items { get; init; } = Array.Empty<RegisterSaleItemRequest>();
}
