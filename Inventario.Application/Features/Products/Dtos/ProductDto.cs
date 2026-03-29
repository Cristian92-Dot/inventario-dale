namespace Inventario.Application.Features.Products.Dtos;

/// <summary>
/// Proyeccion de producto devuelta por la API de inventario.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Identificador unico del producto.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Categoria comercial principal.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Marca o fabricante principal.
    /// </summary>
    public string Brand { get; init; } = string.Empty;

    /// <summary>
    /// Descripcion corta para tarjetas o listados.
    /// </summary>
    public string? ShortDescription { get; init; }

    /// <summary>
    /// Descripcion larga para la vista de detalle.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Precio unitario de venta.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Ruta relativa de la imagen principal del producto.
    /// </summary>
    public string? ImagePath { get; init; }

    /// <summary>
    /// Galeria secundaria del producto.
    /// </summary>
    public IReadOnlyCollection<ProductGalleryImageDto> GalleryImages { get; init; } = Array.Empty<ProductGalleryImageDto>();

    /// <summary>
    /// Stock disponible actual.
    /// </summary>
    public int Stock { get; init; }

    /// <summary>
    /// Umbral minimo de stock.
    /// </summary>
    public int MinStock { get; init; }

    /// <summary>
    /// Indica si el producto actualmente requiere reposicion.
    /// </summary>
    public bool RequiresRestock { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra activo en el catalogo.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Fecha y hora UTC de creacion.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la ultima modificacion, cuando exista.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
