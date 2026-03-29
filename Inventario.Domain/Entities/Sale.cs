using Inventario.Domain.Common;

namespace Inventario.Domain.Entities;

public class Sale : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public decimal Total { get; private set; }
    public string CorrelationId { get; set; } = string.Empty;

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

    public void RecalculateTotal()
    {
        Total = Items.Sum(item => item.Subtotal);
    }
}
