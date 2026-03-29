using System.Security.Cryptography;
using System.Text;
using Inventario.Application.Abstractions.Repositories;
using Inventario.Domain.Entities;

namespace Inventario.Web.Middleware;

public class IdempotencyMiddleware
{
    private const string HeaderName = "Idempotency-Key";
    private static readonly HashSet<string> ProtectedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/sales",
        "/api/v1/auth/refresh"
    };

    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Request.Path.Value ?? string.Empty;
        if (!HttpMethods.IsPost(context.Request.Method) || !ProtectedPaths.Contains(endpoint))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var keyValues) || string.IsNullOrWhiteSpace(keyValues))
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();
        string requestBody;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var requestHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(requestBody)));
        var repository = context.RequestServices.GetRequiredService<IIdempotencyRepository>();
        var unitOfWork = context.RequestServices.GetRequiredService<Inventario.Application.Abstractions.IUnitOfWork>();
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var idempotencyKey = keyValues.ToString();
        context.Items[HttpLoggingContextKeys.IdempotencyKeyHash] = HttpLoggingSanitizer.ComputeFingerprint(idempotencyKey);

        var existing = await repository.GetAsync(idempotencyKey, endpoint, userId, context.RequestAborted);
        if (existing is not null && existing.ExpiresAt > DateTime.UtcNow)
        {
            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "The same Idempotency-Key was used with a different payload.",
                    traceId = System.Diagnostics.Activity.Current?.TraceId.ToString(),
                    correlationId = context.TraceIdentifier,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            context.Response.StatusCode = existing.StatusCode;
            context.Response.ContentType = "application/json";
            context.Items[HttpLoggingContextKeys.IsIdempotencyReplay] = true;
            await context.Response.WriteAsync(existing.ResponseBody);
            return;
        }

        var originalBody = context.Response.Body;
        await using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Position = 0;
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
        memoryStream.Position = 0;

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            await repository.AddAsync(new IdempotencyRecord
            {
                IdempotencyKey = idempotencyKey,
                UserId = userId,
                Endpoint = endpoint,
                RequestHash = requestHash,
                ResponseBody = responseBody,
                StatusCode = context.Response.StatusCode,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            }, context.RequestAborted);

            await unitOfWork.SaveChangesAsync(context.RequestAborted);
        }

        await memoryStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }
}
