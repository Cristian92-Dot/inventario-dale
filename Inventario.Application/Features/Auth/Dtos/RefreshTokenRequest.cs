using System.ComponentModel.DataAnnotations;

namespace Inventario.Application.Features.Auth.Dtos;

/// <summary>
/// Representa el payload de refresh token utilizado para rotacion o revocacion.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Cadena de refresh token persistida y emitida previamente por la API de autenticacion.
    /// </summary>
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
