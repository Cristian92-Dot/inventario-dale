namespace Inventario.Domain.Enums;

public enum SecurityEventType
{
    LoginSucceeded = 1,
    LoginFailed = 2,
    RefreshSucceeded = 3,
    RefreshFailed = 4,
    LogoutSucceeded = 5,
    UnauthorizedAccess = 6,
    ForbiddenAccess = 7,
    RateLimitRejected = 8,
    UnhandledException = 9
}
