using Inventario.Application.Abstractions.Services;
using Inventario.Application.Abstractions.Security;
using Inventario.Application.Features.Auth.Dtos;
using Inventario.Domain.Enums;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Identity;
using Inventario.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ApplicationDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
    }

    public async Task<AuthTokenResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName, cancellationToken)
            ?? throw await CreateAuthExceptionAsync(
                SecurityEventType.LoginFailed,
                LogSeverity.Warning,
                "Invalid credentials.",
                ipAddress,
                cancellationToken);

        if (!user.IsActive)
        {
            throw await CreateAuthExceptionAsync(
                SecurityEventType.LoginFailed,
                LogSeverity.Warning,
                "The user account is inactive.",
                ipAddress,
                cancellationToken,
                user);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (!signInResult.Succeeded)
        {
            throw await CreateAuthExceptionAsync(
                SecurityEventType.LoginFailed,
                LogSeverity.Warning,
                "Invalid credentials.",
                ipAddress,
                cancellationToken,
                user);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.UserName ?? user.Email ?? "user", roles);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedBy = user.UserName,
            CreatedByIp = ipAddress
        };

        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            ActionType = AuditActionType.Login,
            EntityName = nameof(ApplicationUser),
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _dbContext.SecurityEvents.AddAsync(new SecurityEvent
        {
            EventType = SecurityEventType.LoginSucceeded,
            Severity = LogSeverity.Information,
            Message = "User login completed successfully.",
            UserId = user.Id,
            UserName = user.UserName,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(20),
            UserName = user.UserName ?? string.Empty,
            Roles = roles.ToArray()
        };
    }

    public async Task<AuthTokenResponse> RefreshAsync(RefreshTokenRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var currentToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken)
            ?? throw await CreateAuthExceptionAsync(
                SecurityEventType.RefreshFailed,
                LogSeverity.Warning,
                "Refresh token is invalid.",
                ipAddress,
                cancellationToken);

        if (!currentToken.IsActive)
        {
            throw await CreateAuthExceptionAsync(
                SecurityEventType.RefreshFailed,
                LogSeverity.Warning,
                "Refresh token is inactive.",
                ipAddress,
                cancellationToken,
                userId: currentToken.UserId);
        }

        var user = await _userManager.FindByIdAsync(currentToken.UserId)
            ?? throw await CreateAuthExceptionAsync(
                SecurityEventType.RefreshFailed,
                LogSeverity.Warning,
                "Refresh token user was not found.",
                ipAddress,
                cancellationToken,
                userId: currentToken.UserId);

        currentToken.RevokedAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.UserName ?? user.Email ?? "user", roles);
        var nextRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var nextRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = nextRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedBy = user.UserName,
            CreatedByIp = ipAddress
        };

        await _dbContext.RefreshTokens.AddAsync(nextRefreshToken, cancellationToken);
        await _dbContext.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            UserName = user.UserName,
            ActionType = AuditActionType.SecurityEvent,
            EntityName = nameof(RefreshToken),
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _dbContext.SecurityEvents.AddAsync(new SecurityEvent
        {
            EventType = SecurityEventType.RefreshSucceeded,
            Severity = LogSeverity.Information,
            Message = "Refresh token rotation completed successfully.",
            UserId = user.Id,
            UserName = user.UserName,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = nextRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(20),
            UserName = user.UserName ?? string.Empty,
            Roles = roles.ToArray()
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);
        if (token is null)
        {
            return;
        }

        token.RevokedAt = DateTime.UtcNow;
        var user = await _userManager.FindByIdAsync(token.UserId);
        await _dbContext.AuditLogs.AddAsync(new AuditLog
        {
            UserId = token.UserId,
            UserName = user?.UserName,
            ActionType = AuditActionType.Logout,
            EntityName = nameof(ApplicationUser),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _dbContext.SecurityEvents.AddAsync(new SecurityEvent
        {
            EventType = SecurityEventType.LogoutSucceeded,
            Severity = LogSeverity.Information,
            Message = "User logout completed successfully.",
            UserId = token.UserId,
            UserName = user?.UserName,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<UnauthorizedAccessException> CreateAuthExceptionAsync(
        SecurityEventType eventType,
        LogSeverity severity,
        string message,
        string ipAddress,
        CancellationToken cancellationToken,
        ApplicationUser? user = null,
        string? userId = null)
    {
        await _dbContext.SecurityEvents.AddAsync(new SecurityEvent
        {
            EventType = eventType,
            Severity = severity,
            Message = message,
            UserId = user?.Id ?? userId,
            UserName = user?.UserName,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new UnauthorizedAccessException(message);
    }
}
