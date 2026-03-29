using Inventario.Application.Features.Users.Dtos;

namespace Inventario.Application.Abstractions.Services;

public interface IUserManagementService
{
    Task<IReadOnlyCollection<UserManagementDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserManagementDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserManagementDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserManagementDto> UpdateProfileAsync(string userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserManagementDto> UpdateRoleAsync(string userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
    Task ToggleStatusAsync(string userId, CancellationToken cancellationToken = default);
}
