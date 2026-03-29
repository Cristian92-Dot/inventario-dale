using Inventario.Domain.Entities;

namespace Inventario.Application.Abstractions.Repositories;

public interface IIdempotencyRepository
{
    Task<IdempotencyRecord?> GetAsync(string key, string endpoint, string? userId, CancellationToken cancellationToken = default);
    Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);
}
