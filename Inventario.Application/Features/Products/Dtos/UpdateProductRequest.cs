using System.ComponentModel.DataAnnotations;

namespace Inventario.Application.Features.Products.Dtos;

/// <summary>
/// Payload utilizado para actualizar la informacion comercial y de inventario de un producto existente.
/// </summary>
public class UpdateProductRequest : IProductInput
{
    /// <summary>
    /// Nombre comercial actualizado del producto.
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
    /// Precio unitario actualizado de venta.
    /// </summary>
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Price { get; set; }

    /// <summary>
    /// Nuevo nivel de stock disponible despues de ajustes o conciliaciones.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>
    /// Nuevo umbral minimo de stock.
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
