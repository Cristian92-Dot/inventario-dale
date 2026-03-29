namespace Inventario.Application.Features.Auth.Dtos;

/// <summary>
/// Resultado de autenticacion devuelto despues del login o de la rotacion del refresh token.
/// </summary>
public class AuthTokenResponse
{
    /// <summary>
    /// Token de acceso JWT de corta duracion para llamadas protegidas de la API.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Refresh token persistido utilizado para obtener un nuevo access token cuando el actual expira.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Fecha y hora UTC de expiracion del access token.
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Nombre de usuario asociado al principal autenticado.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Roles efectivos asignados al usuario autenticado.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
}
