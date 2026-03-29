using FluentAssertions;
using Inventario.Application.Abstractions;
using Inventario.Application.Abstractions.Repositories;
using Inventario.Application.Common;
using Inventario.Application.Features.Sales.Dtos;
using Inventario.Application.Services;
using Inventario.Domain.Entities;
using Moq;

namespace Inventario.Tests.Unit;

public class SaleServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenProductStockIsInsufficient()
    {
        var product = new Product { Name = "Laptop", Price = 1000m };
        product.ConfigureInventory(1, 1);

        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var saleRepository = new Mock<ISaleRepository>();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser
            .Setup(x => x.GetCurrent())
            .Returns(new CurrentRequestContext { UserId = "user-1", UserName = "empleado" });

        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.SetupGet(x => x.UtcNow).Returns(DateTime.UtcNow);

        var unitOfWork = new Mock<IUnitOfWork>();

        var service = new SaleService(productRepository.Object, saleRepository.Object, currentUser.Object, dateTimeProvider.Object, unitOfWork.Object);

        var action = async () => await service.RegisterAsync(new RegisterSaleRequest
        {
            Items = new[]
            {
                new RegisterSaleItemRequest { ProductId = product.Id, Quantity = 2 }
            }
        });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*stock suficiente*");
    }

    [Fact]
    public async Task RegisterAsync_ShouldDeductStockAndMarkRestock_WhenSaleIsValid()
    {
        var product = new Product { Name = "Teclado", Price = 50m };
        product.ConfigureInventory(3, 2);

        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Sale? savedSale = null;
        var saleRepository = new Mock<ISaleRepository>();
        saleRepository
            .Setup(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => savedSale = sale)
            .Returns(Task.CompletedTask);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser
            .Setup(x => x.GetCurrent())
            .Returns(new CurrentRequestContext { UserId = "user-1", UserName = "empleado", CorrelationId = "corr-123" });

        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.SetupGet(x => x.UtcNow).Returns(new DateTime(2026, 3, 27, 12, 0, 0, DateTimeKind.Utc));

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new SaleService(productRepository.Object, saleRepository.Object, currentUser.Object, dateTimeProvider.Object, unitOfWork.Object);

        var result = await service.RegisterAsync(new RegisterSaleRequest
        {
            Items = new[]
            {
                new RegisterSaleItemRequest { ProductId = product.Id, Quantity = 2 }
            }
        });

        result.Total.Should().Be(100m);
        product.Stock.Should().Be(1);
        product.RequiresRestock.Should().BeTrue();
        savedSale.Should().NotBeNull();
        savedSale!.CorrelationId.Should().Be("corr-123");
    }
}
