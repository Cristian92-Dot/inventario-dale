using Inventario.Domain.Enums;

namespace Inventario.Domain.Entities;

public class SecurityEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public SecurityEventType EventType { get; set; }
    public LogSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
