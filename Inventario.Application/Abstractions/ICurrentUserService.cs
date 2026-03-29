using Inventario.Application.Common;

namespace Inventario.Application.Abstractions;

public interface ICurrentUserService
{
    CurrentRequestContext GetCurrent();
}
