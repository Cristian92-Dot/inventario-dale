namespace Inventario.Application.Features.Products.Dtos;

public class ProductGalleryImageDto
{
    public Guid Id { get; init; }
    public string ImagePath { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}
