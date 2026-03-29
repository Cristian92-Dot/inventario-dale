using Inventario.Domain.Common;

namespace Inventario.Domain.Entities;

public class ProductCategory : SoftDeletableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
