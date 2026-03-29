using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Inventario.Web.Controllers.V1;

/// <summary>
/// Gestiona el acceso a la API mediante autenticación con JWT y refresh tokens.
/// </summary>
/// <remarks>
/// Este controlador centraliza el inicio de sesión, la renovación de tokens y el cierre de sesión de la API.
/// Está pensado para clientes técnicos, pruebas desde Swagger e integraciones externas que consumen los endpoints protegidos.
/// </remarks>
[EnableRateLimiting("auth-policy")]
[AllowAnonymous]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Valida las credenciales del usuario y devuelve los tokens necesarios para consumir la API.
    /// </summary>
    /// <remarks>
    /// Este es el endpoint principal de acceso a la API.
    /// Cuando el inicio de sesión es correcto, se valida el estado de la cuenta y se devuelve un access token JWT junto con su refresh token.
    /// </remarks>
    /// <param name="request">Credenciales del usuario: nombre de usuario y contraseña.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Respuesta con access token, refresh token, expiración y roles del usuario.</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _authService.LoginAsync(request, ipAddress, cancellationToken);
        return Ok(Success(result, "Inicio de sesión completado correctamente."));
    }

    /// <summary>
    /// Renueva la sesión API usando un refresh token válido.
    /// </summary>
    /// <remarks>
    /// Si el refresh token recibido sigue activo, se revoca y se entrega un nuevo par de tokens.
    /// Esto permite mantener la sesión sin volver a pedir usuario y contraseña.
    /// </remarks>
    /// <param name="request">Token de refresco que se desea renovar.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Respuesta con el nuevo access token y el nuevo refresh token.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _authService.RefreshAsync(request, ipAddress, cancellationToken);
        return Ok(Success(result, "Token renovado correctamente."));
    }

    /// <summary>
    /// Revoca el refresh token recibido y cierra la sesión API.
    /// </summary>
    /// <remarks>
    /// Después de esta operación, el refresh token ya no podrá volver a usarse.
    /// Esto evita que una sesión cerrada siga renovándose en segundo plano.
    /// </remarks>
    /// <param name="request">Refresh token que debe ser revocado.</param>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Respuesta que confirma el cierre de sesión.</returns>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Ok(Success(new { }, "Cierre de sesión completado correctamente."));
    }
}
