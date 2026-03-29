using System.Diagnostics;
using System.Security.Claims;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;
using Inventario.Infrastructure.Persistence;

namespace Inventario.Web.Middleware;

public class RequestTraceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTraceMiddleware> _logger;

    public RequestTraceMiddleware(RequestDelegate next, ILogger<RequestTraceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestTraceId = Guid.NewGuid();
        context.Items[HttpLoggingContextKeys.RequestTraceId] = requestTraceId;

        await _next(context);

        stopwatch.Stop();
        context.Items["RequestDurationMs"] = stopwatch.ElapsedMilliseconds;

        if (context.Request.Path.StartsWithSegments("/lib") || context.Request.Path.StartsWithSegments("/css") || context.Request.Path.StartsWithSegments("/js"))
        {
            return;
        }

        try
        {
            var dbContext = context.RequestServices.GetService<ApplicationDbContext>();
            if (dbContext is null)
            {
                return;
            }

            var trace = new RequestTrace
            {
                Id = requestTraceId,
                TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty,
                CorrelationId = context.TraceIdentifier,
                UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName = context.User.Identity?.Name,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                DurationMs = stopwatch.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.RequestTraces.Add(trace);

            var securityEvent = BuildSecurityEvent(context);
            if (securityEvent is not null)
            {
                context.Items[HttpLoggingContextKeys.HasSecurityEvent] = true;
                dbContext.SecurityEvents.Add(securityEvent);
            }

            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to persist request trace for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
    }

    private static SecurityEvent? BuildSecurityEvent(HttpContext context)
    {
        var statusCode = context.Response.StatusCode;
        if (statusCode is not (StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden or StatusCodes.Status429TooManyRequests))
        {
            return null;
        }

        var eventType = statusCode switch
        {
            StatusCodes.Status401Unauthorized => SecurityEventType.UnauthorizedAccess,
            StatusCodes.Status403Forbidden => SecurityEventType.ForbiddenAccess,
            StatusCodes.Status429TooManyRequests => SecurityEventType.RateLimitRejected,
            _ => SecurityEventType.UnauthorizedAccess
        };

        var severity = statusCode == StatusCodes.Status429TooManyRequests
            ? LogSeverity.Warning
            : LogSeverity.Error;

        return new SecurityEvent
        {
            EventType = eventType,
            Severity = severity,
            Message = $"HTTP {statusCode} on {context.Request.Method} {context.Request.Path}",
            UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = context.User.Identity?.Name,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            Path = context.Request.Path,
            Method = context.Request.Method,
            CorrelationId = context.TraceIdentifier,
            CreatedAt = DateTime.UtcNow
        };
    }
}
