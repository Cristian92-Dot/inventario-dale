namespace Inventario.Application.Features.Sales.Dtos;

/// <summary>
/// Modelo de lectura que representa una linea persistida de venta.
/// </summary>
public class SaleItemDto
{
    /// <summary>
    /// Identificador del producto asociado a la linea de venta.
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Nombre del producto resuelto al momento de la venta para fines de trazabilidad.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// Cantidad vendida en la linea.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Precio unitario aplicado a la linea.
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Subtotal monetario de la linea.
    /// </summary>
    public decimal Subtotal { get; init; }
}
