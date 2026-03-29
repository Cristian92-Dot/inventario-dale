namespace Inventario.Domain.Entities;

public class HttpTransactionLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? RequestTraceId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string? EndpointName { get; set; }
    public string? RoutePattern { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? QueryStringMasked { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestContentType { get; set; }
    public long? RequestSizeBytes { get; set; }
    public string? RequestHeadersJson { get; set; }
    public string? RequestBodyMasked { get; set; }
    public string? RequestFingerprint { get; set; }
    public string RequestCaptureStatus { get; set; } = string.Empty;
    public bool IsRequestTruncated { get; set; }
    public string? ResponseContentType { get; set; }
    public long? ResponseSizeBytes { get; set; }
    public string? ResponseHeadersJson { get; set; }
    public string? ResponseBodyMasked { get; set; }
    public string? ResponseFingerprint { get; set; }
    public string ResponseCaptureStatus { get; set; } = string.Empty;
    public bool IsResponseTruncated { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public bool IsIdempotencyReplay { get; set; }
    public string? IdempotencyKeyHash { get; set; }
    public bool HasSecurityEvent { get; set; }
    public string? ExceptionType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
