namespace Inventario.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
