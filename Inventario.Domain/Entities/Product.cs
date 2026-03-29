using Inventario.Domain.Common;

namespace Inventario.Domain.Entities;

public class Product : SoftDeletableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public int Stock { get; private set; }
    public int MinStock { get; private set; }
    public bool RequiresRestock { get; private set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<ProductGalleryImage> GalleryImages { get; set; } = new List<ProductGalleryImage>();

    public void ReplaceGalleryImages(IEnumerable<string> imagePaths, string? createdBy, DateTime now)
    {
        GalleryImages.Clear();

        var sortOrder = 0;
        foreach (var imagePath in imagePaths.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct())
        {
            GalleryImages.Add(new ProductGalleryImage
            {
                ProductId = Id,
                ImagePath = imagePath,
                SortOrder = sortOrder++,
                CreatedAt = now,
                CreatedBy = createdBy
            });
        }
    }

    public void ConfigureInventory(int stock, int minStock)
    {
        if (stock < 0)
        {
            throw new InvalidOperationException("El stock no puede ser negativo.");
        }

        if (minStock < 0)
        {
            throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
        }

        Stock = stock;
        MinStock = minStock;
        RequiresRestock = Stock <= MinStock;
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
        }

        if (Stock < quantity)
        {
            throw new InvalidOperationException($"No hay stock suficiente para el producto '{Name}'.");
        }

        Stock -= quantity;
        RequiresRestock = Stock <= MinStock;
    }

    public void RestoreStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
        }

        Stock += quantity;
        RequiresRestock = Stock <= MinStock;
    }
}
