namespace Inventario.Application.Features.Users.Dtos;

/// <summary>
/// Proyeccion administrativa de un usuario gestionado por la plataforma.
/// </summary>
public class UserManagementDto
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? AvatarPath { get; init; }
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int FailedLoginAttempts { get; init; }
    public DateTime? LockoutEndUtc { get; init; }
    public DateTime CreatedAt { get; init; }
}
