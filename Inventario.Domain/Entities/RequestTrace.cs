namespace Inventario.Domain.Entities;

public class RequestTrace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TraceId { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
