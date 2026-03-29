namespace Inventario.Application.Abstractions.Security;

public interface IJwtTokenService
{
    string GenerateAccessToken(string userId, string userName, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
