namespace Inventario.Application.Features.Products.Dtos;

public class ProductOptionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}
