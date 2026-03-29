using Inventario.Application.Abstractions.Services;
using Inventario.Application.Common.Exceptions;
using Inventario.Application.Features.Users.Dtos;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;
using Inventario.Infrastructure.Identity;
using Inventario.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public UserManagementService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<UserManagementDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
        {
            throw new ConflictException("Ya existe un usuario con ese nombre.");
        }

        var existingEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmail is not null)
        {
            throw new ConflictException("Ya existe un usuario con ese correo.");
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            DisplayName = request.DisplayName.Trim(),
            Email = request.Email.Trim(),
            AvatarPath = request.AvatarPath,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(string.Join(" ", result.Errors.Select(x => x.Description)));
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        await _dbContext.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            ActionType = AuditActionType.Create,
            EntityName = nameof(ApplicationUser),
            NewValuesJson = System.Text.Json.JsonSerializer.Serialize(new { user.UserName, user.Email, Role = request.Role }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapAsync(user);
    }

    public async Task<IReadOnlyCollection<UserManagementDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .OrderBy(x => x.UserName)
            .ToListAsync(cancellationToken);

        var result = new List<UserManagementDto>(users.Count);
        foreach (var user in users)
        {
            result.Add(await MapAsync(user));
        }

        return result;
    }

    public async Task<UserManagementDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return await MapAsync(user);
    }

    public async Task<UserManagementDto> UpdateProfileAsync(string userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        var normalizedUserName = request.UserName.Trim();
        var normalizedEmail = request.Email.Trim();

        var existingUser = await _userManager.FindByNameAsync(normalizedUserName);
        if (existingUser is not null && existingUser.Id != userId)
        {
            throw new ConflictException("Ya existe un usuario con ese nombre.");
        }

        var existingEmail = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingEmail is not null && existingEmail.Id != userId)
        {
            throw new ConflictException("Ya existe un usuario con ese correo.");
        }

        user.UserName = normalizedUserName;
        user.DisplayName = request.DisplayName.Trim();
        user.Email = normalizedEmail;
        user.AvatarPath = request.AvatarPath;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new BusinessRuleException(string.Join(" ", result.Errors.Select(x => x.Description)));
        }

        await _dbContext.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            ActionType = AuditActionType.Update,
            EntityName = nameof(ApplicationUser),
            NewValuesJson = System.Text.Json.JsonSerializer.Serialize(new { user.UserName, user.DisplayName, user.Email, user.AvatarPath }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapAsync(user);
    }

    public async Task ToggleStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        user.IsActive = !user.IsActive;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new BusinessRuleException(string.Join(" ", updateResult.Errors.Select(x => x.Description)));
        }

        await _dbContext.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            ActionType = AuditActionType.Update,
            EntityName = nameof(ApplicationUser),
            NewValuesJson = System.Text.Json.JsonSerializer.Serialize(new { user.IsActive }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserManagementDto> UpdateRoleAsync(string userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        await _dbContext.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            ActionType = AuditActionType.Update,
            EntityName = nameof(ApplicationUser),
            NewValuesJson = System.Text.Json.JsonSerializer.Serialize(new { Role = request.Role }),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapAsync(user);
    }

    private async Task<UserManagementDto> MapAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserManagementDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName ?? string.Empty : user.DisplayName,
            Email = user.Email ?? string.Empty,
            AvatarPath = user.AvatarPath,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockoutEndUtc = user.LockoutEndUtc,
            CreatedAt = user.CreatedAt
        };
    }
}
