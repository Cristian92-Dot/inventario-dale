namespace Inventario.Application.Common;

public class CurrentRequestContext
{
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? CorrelationId { get; init; }
    public string? TraceId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}
