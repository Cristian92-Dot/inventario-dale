using Inventario.Application.Features.Auth.Dtos;

namespace Inventario.Application.Abstractions.Services;

public interface IAuthService
{
    Task<AuthTokenResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthTokenResponse> RefreshAsync(RefreshTokenRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
