using Inventario.Domain.Common;

namespace Inventario.Domain.Entities;

public class ProductGalleryImage : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Product? Product { get; set; }
}
