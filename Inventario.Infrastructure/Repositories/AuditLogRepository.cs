using Inventario.Application.Abstractions.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AuditLog>> GetRecentAsync(int top, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(top)
            .ToListAsync(cancellationToken);
    }
}
