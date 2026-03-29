using System.ComponentModel.DataAnnotations;

namespace Inventario.Application.Features.Users.Dtos;

/// <summary>
/// Payload utilizado para modificar el rol principal de un usuario.
/// </summary>
public class UpdateUserRoleRequest
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Role { get; init; } = string.Empty;
}
