using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;

namespace Inventario.Web.Middleware;

public class HttpTransactionLoggingMiddleware
{
    private const int MaxRequestBodyLength = 16_384;
    private const int MaxResponseBodyLength = 32_768;
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpTransactionLoggingMiddleware> _logger;

    public HttpTransactionLoggingMiddleware(RequestDelegate next, ILogger<HttpTransactionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsStaticAsset(context.Request.Path))
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();

        var requestBody = await ReadBodyAsync(context.Request.Body, context.Request.ContentLength);
        context.Request.Body.Position = 0;

        var originalResponseBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);
        }
        finally
        {
            responseBuffer.Position = 0;
            var responseBody = await ReadBodyAsync(responseBuffer, responseBuffer.Length);
            responseBuffer.Position = 0;

            await PersistHttpTransactionAsync(context, requestBody, responseBody);

            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    private async Task PersistHttpTransactionAsync(HttpContext context, string rawRequestBody, string rawResponseBody)
    {
        try
        {
            var dbContext = context.RequestServices.GetService<ApplicationDbContext>();
            if (dbContext is null)
            {
                return;
            }

            var maskedRequestBody = HttpLoggingSanitizer.MaskBody(rawRequestBody, context.Request.ContentType);
            var (requestBodyStored, isRequestTruncated) = Truncate(maskedRequestBody, MaxRequestBodyLength);
            var maskedResponseBody = HttpLoggingSanitizer.MaskBody(rawResponseBody, context.Response.ContentType);
            var (responseBodyStored, isResponseTruncated) = Truncate(maskedResponseBody, MaxResponseBodyLength);

            var endpoint = context.GetEndpoint();
            var routePattern = endpoint is RouteEndpoint routeEndpoint ? routeEndpoint.RoutePattern.RawText : null;

            dbContext.HttpTransactionLogs.Add(new HttpTransactionLog
            {
                RequestTraceId = context.Items.TryGetValue(HttpLoggingContextKeys.RequestTraceId, out var requestTraceId) && requestTraceId is Guid traceId ? traceId : null,
                CorrelationId = context.TraceIdentifier,
                TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty,
                EndpointName = endpoint?.DisplayName,
                RoutePattern = routePattern,
                Path = context.Request.Path,
                Method = context.Request.Method,
                QueryStringMasked = HttpLoggingSanitizer.MaskQueryString(context.Request.QueryString.Value),
                UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName = context.User.Identity?.Name,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                RequestContentType = context.Request.ContentType,
                RequestSizeBytes = context.Request.ContentLength,
                RequestHeadersJson = HttpLoggingSanitizer.MaskHeaders(context.Request.Headers),
                RequestBodyMasked = requestBodyStored,
                RequestFingerprint = HttpLoggingSanitizer.ComputeFingerprint(maskedRequestBody),
                RequestCaptureStatus = string.IsNullOrWhiteSpace(rawRequestBody) ? "SinCuerpo" : "Capturado",
                IsRequestTruncated = isRequestTruncated,
                ResponseContentType = context.Response.ContentType,
                ResponseSizeBytes = Encoding.UTF8.GetByteCount(rawResponseBody),
                ResponseHeadersJson = SerializeResponseHeaders(context.Response.Headers),
                ResponseBodyMasked = responseBodyStored,
                ResponseFingerprint = HttpLoggingSanitizer.ComputeFingerprint(maskedResponseBody),
                ResponseCaptureStatus = string.IsNullOrWhiteSpace(rawResponseBody) ? "SinCuerpo" : "Capturado",
                IsResponseTruncated = isResponseTruncated,
                StatusCode = context.Response.StatusCode,
                DurationMs = GetDurationMs(context),
                IsIdempotencyReplay = context.Items.TryGetValue(HttpLoggingContextKeys.IsIdempotencyReplay, out var isReplay) && isReplay is true,
                IdempotencyKeyHash = context.Items.TryGetValue(HttpLoggingContextKeys.IdempotencyKeyHash, out var idempotencyKeyHash) ? idempotencyKeyHash?.ToString() : null,
                HasSecurityEvent = context.Items.TryGetValue(HttpLoggingContextKeys.HasSecurityEvent, out var hasSecurityEvent) && hasSecurityEvent is true,
                ExceptionType = context.Items.TryGetValue(HttpLoggingContextKeys.ExceptionType, out var exceptionType) ? exceptionType?.ToString() : null,
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "No se pudo persistir el log HTTP para {Method} {Path}", context.Request.Method, context.Request.Path);
        }

    }

    private static string SerializeResponseHeaders(IHeaderDictionary headers)
    {
        var sanitized = headers.ToDictionary(
            item => item.Key,
            item => item.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
                ? HttpLoggingSanitizer.MaskTokenValue(item.Value.ToString())
                : item.Value.ToString());

        return System.Text.Json.JsonSerializer.Serialize(sanitized);
    }

    private static long GetDurationMs(HttpContext context)
    {
        return context.Items.TryGetValue("RequestDurationMs", out var duration) && duration is long elapsed
            ? elapsed
            : 0L;
    }

    private static async Task<string> ReadBodyAsync(Stream stream, long? length)
    {
        if (stream is null)
        {
            return string.Empty;
        }

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        stream.Position = 0;
        return body;
    }

    private static (string Content, bool IsTruncated) Truncate(string content, int maxLength)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
        {
            return (content, false);
        }

        return ($"{content[..maxLength]}...[truncado]", true);
    }

    private static bool IsStaticAsset(PathString path)
    {
        return path.StartsWithSegments("/lib") || path.StartsWithSegments("/css") || path.StartsWithSegments("/js") || path.StartsWithSegments("/favicon.ico");
    }
}
