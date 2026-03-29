using Inventario.Application.Abstractions;
using Inventario.Application.Abstractions.Repositories;
using Inventario.Application.Abstractions.Services;
using Inventario.Application.Common.Exceptions;
using Inventario.Application.Features.Sales.Dtos;
using Inventario.Domain.Entities;

namespace Inventario.Application.Services;

public class SaleService : ISaleService
{
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SaleService(
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<SaleDto> RegisterAsync(RegisterSaleRequest request, CancellationToken cancellationToken = default)
    {
        var context = _currentUserService.GetCurrent();
        if (string.IsNullOrWhiteSpace(context.UserId))
        {
            throw new ForbiddenException("Debes contar con una sesión activa para registrar ventas.");
        }

        var sale = new Sale
        {
            UserId = context.UserId,
            Date = _dateTimeProvider.UtcNow,
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedBy = context.UserName,
            CorrelationId = context.CorrelationId ?? string.Empty
        };

        foreach (var itemRequest in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemRequest.ProductId, cancellationToken)
                ?? throw new NotFoundException("Uno de los productos seleccionados no existe o ya no está disponible.");

            product.DeductStock(itemRequest.Quantity);

            var saleItem = new SaleItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price,
                Subtotal = product.Price * itemRequest.Quantity
            };

            sale.Items.Add(saleItem);
        }

        sale.RecalculateTotal();

        await _saleRepository.AddAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SaleDto
        {
            Id = sale.Id,
            Date = sale.Date,
            UserId = sale.UserId,
            Total = sale.Total,
            CorrelationId = sale.CorrelationId,
            Items = sale.Items.Select(item => new SaleItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal
            }).ToArray()
        };
    }
}
