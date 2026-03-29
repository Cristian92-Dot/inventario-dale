namespace Inventario.Application.Features.Products.Dtos;

/// <summary>
/// Parametros de consulta utilizados para filtrar y paginar el listado de productos.
/// </summary>
public class ProductQueryRequest
{
    /// <summary>
    /// Termino opcional de busqueda aplicado sobre el nombre del producto.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Categoria opcional para filtrar el catálogo.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Marca opcional para filtrar el catálogo.
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Estado opcional de inventario: available, lowstock o outofstock.
    /// </summary>
    public string? StockStatus { get; set; }

    /// <summary>
    /// Ordenamiento opcional de resultados.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Pagina solicitada. Los valores menores que 1 son normalizados por la capa de aplicacion.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamano de pagina solicitado. La aplicacion impone un limite superior para proteger la API.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
