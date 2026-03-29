using System.ComponentModel.DataAnnotations;

namespace Inventario.Application.Features.Products.Dtos;

/// <summary>
/// Payload utilizado para crear un nuevo producto en el catalogo de inventario.
/// </summary>
public class CreateProductRequest : IProductInput
{
    /// <summary>
    /// Nombre comercial del producto. Debe ser unico entre los productos activos.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Categoria comercial principal del producto.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Marca o fabricante principal del producto.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Descripcion corta utilizada en tarjetas y vistas resumidas.
    /// </summary>
    [MaxLength(240)]
    public string? ShortDescription { get; set; }

    /// <summary>
    /// Descripcion larga del producto orientada al detalle enriquecido.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Precio unitario de venta almacenado por el catalogo.
    /// </summary>
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Price { get; set; }

    /// <summary>
    /// Stock disponible al momento de la creacion.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>
    /// Umbral minimo de stock utilizado para determinar alertas de reposicion.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MinStock { get; set; }

    /// <summary>
    /// Ruta relativa de la imagen principal del producto dentro de la aplicacion.
    /// </summary>
    [MaxLength(300)]
    public string? ImagePath { get; set; }

    /// <summary>
    /// Galeria secundaria del producto.
    /// </summary>
    public IReadOnlyCollection<string> GalleryImagePaths { get; set; } = Array.Empty<string>();
}
