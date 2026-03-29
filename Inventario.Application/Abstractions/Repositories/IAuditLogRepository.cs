using Inventario.Domain.Entities;

namespace Inventario.Application.Abstractions.Repositories;

public interface IAuditLogRepository
{
    Task<IReadOnlyCollection<AuditLog>> GetRecentAsync(int top, CancellationToken cancellationToken = default);
}
