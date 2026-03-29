namespace Inventario.Domain.Common;

public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsActive { get; set; } = true;
}
