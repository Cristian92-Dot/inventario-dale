namespace Inventario.Domain.Entities;

public class IdempotencyRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}
