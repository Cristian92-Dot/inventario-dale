using Inventario.Application.Abstractions.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly ApplicationDbContext _dbContext;

    public IdempotencyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        await _dbContext.IdempotencyRecords.AddAsync(record, cancellationToken);
    }

    public async Task<IdempotencyRecord?> GetAsync(string key, string endpoint, string? userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.IdempotencyRecords
            .FirstOrDefaultAsync(x => x.IdempotencyKey == key && x.Endpoint == endpoint && x.UserId == userId, cancellationToken);
    }
}
