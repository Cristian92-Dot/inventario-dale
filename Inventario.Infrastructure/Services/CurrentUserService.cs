using System.Diagnostics;
using System.Security.Claims;
using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Inventario.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentRequestContext GetCurrent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return new CurrentRequestContext
        {
            UserId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = httpContext?.User.Identity?.Name,
            CorrelationId = httpContext?.TraceIdentifier,
            TraceId = Activity.Current?.TraceId.ToString(),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString()
        };
    }
}
