using System.ComponentModel.DataAnnotations;

namespace Inventario.Application.Features.Categories.Dtos;

public class CreateProductCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(240)]
    public string? Description { get; set; }
}
