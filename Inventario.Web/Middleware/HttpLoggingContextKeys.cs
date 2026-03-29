namespace Inventario.Web.Middleware;

public static class HttpLoggingContextKeys
{
    public const string RequestTraceId = "RequestTraceId";
    public const string HasSecurityEvent = "HasSecurityEvent";
    public const string ExceptionType = "ExceptionType";
    public const string IsIdempotencyReplay = "IsIdempotencyReplay";
    public const string IdempotencyKeyHash = "IdempotencyKeyHash";
}
