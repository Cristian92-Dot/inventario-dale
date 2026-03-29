using System.ComponentModel.DataAnnotations;

namespace Inventario.Application.Features.Users.Dtos;

public class UpdateUserProfileRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre de usuario no puede superar los 100 caracteres.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre visible es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre visible no puede superar los 150 caracteres.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo ingresado no es válido.")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "La ruta del avatar no puede superar los 300 caracteres.")]
    public string? AvatarPath { get; set; }
}
