using System.ComponentModel.DataAnnotations;

namespace Inventario.Application.Features.Sales.Dtos;

/// <summary>
/// Representa una linea individual dentro de una solicitud de registro de venta.
/// </summary>
public class RegisterSaleItemRequest
{
    /// <summary>
    /// Identificador del producto que sera vendido.
    /// </summary>
    [Required]
    public Guid ProductId { get; init; }

    /// <summary>
    /// Cantidad de unidades solicitadas para el producto referenciado.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}
