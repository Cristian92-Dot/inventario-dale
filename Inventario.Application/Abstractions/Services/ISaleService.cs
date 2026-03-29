using Inventario.Application.Features.Sales.Dtos;

namespace Inventario.Application.Abstractions.Services;

public interface ISaleService
{
    Task<SaleDto> RegisterAsync(RegisterSaleRequest request, CancellationToken cancellationToken = default);
}
