using System.ComponentModel.DataAnnotations;

namespace Inventario.Application.Features.Auth.Dtos;

/// <summary>
/// Representa el payload de credenciales utilizado para autenticar un usuario API.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Nombre de usuario unico registrado en el almacén de identidad.
    /// </summary>
    [Required]
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Contrasena en texto plano enviada por el cliente para su validacion.
    /// </summary>
    [Required]
    public string Password { get; init; } = string.Empty;
}
