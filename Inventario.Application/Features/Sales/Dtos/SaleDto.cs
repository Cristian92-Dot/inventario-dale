namespace Inventario.Application.Features.Sales.Dtos;

/// <summary>
/// Resumen devuelto despues de registrar correctamente una venta.
/// </summary>
public class SaleDto
{
    /// <summary>
    /// Identificador unico de la venta.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que se registro la venta.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Identificador del usuario que realizo la venta.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Monto total calculado por el servidor para la venta.
    /// </summary>
    public decimal Total { get; init; }

    /// <summary>
    /// Identificador de correlacion asociado a la solicitud HTTP que creo la venta.
    /// </summary>
    public string CorrelationId { get; init; } = string.Empty;

    /// <summary>
    /// Lineas persistidas incluidas dentro de la venta.
    /// </summary>
    public IReadOnlyCollection<SaleItemDto> Items { get; init; } = Array.Empty<SaleItemDto>();
}
