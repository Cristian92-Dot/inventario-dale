namespace Inventario.Application.Features.Products.Dtos;

public interface IProductInput
{
    string Name { get; }
    string Category { get; }
    string Brand { get; }
    string? ShortDescription { get; }
    string? Description { get; }
    decimal Price { get; }
    int Stock { get; }
    int MinStock { get; }
}
