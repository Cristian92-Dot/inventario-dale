namespace Inventario.Application.Features.Categories.Dtos;

public class ProductCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}
