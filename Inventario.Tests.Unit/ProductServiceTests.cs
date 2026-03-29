using FluentAssertions;
using Inventario.Application.Abstractions;
using Inventario.Application.Abstractions.Repositories;
using Inventario.Application.Common;
using Inventario.Application.Common.Exceptions;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Application.Services;
using Inventario.Domain.Entities;
using Moq;

namespace Inventario.Tests.Unit;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldMarkProductAsRequiringRestock_WhenStockIsAtMinimum()
    {
        var repository = new Mock<IProductRepository>();
        repository
            .Setup(x => x.GetActiveByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        Product? createdProduct = null;
        repository
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => createdProduct = product)
            .Returns(Task.CompletedTask);

        var categoryRepository = new Mock<IProductCategoryRepository>();
        categoryRepository
            .Setup(x => x.GetByNameAsync("Perifericos", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductCategory { Name = "Perifericos", IsActive = true });

        var saleRepository = new Mock<ISaleRepository>();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser
            .Setup(x => x.GetCurrent())
            .Returns(new CurrentRequestContext { UserName = "admin" });

        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.SetupGet(x => x.UtcNow).Returns(new DateTime(2026, 3, 27, 12, 0, 0, DateTimeKind.Utc));

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new ProductService(repository.Object, categoryRepository.Object, saleRepository.Object, currentUser.Object, dateTimeProvider.Object, unitOfWork.Object);

        var result = await service.CreateAsync(new CreateProductRequest
        {
            Name = "Mouse Corporativo",
            Category = "Perifericos",
            Brand = "ComfortLine",
            ShortDescription = "Mouse ergonómico para uso diario.",
            Description = "Mouse diseñado para oficinas con enfoque en comodidad y precisión.",
            Price = 25m,
            Stock = 5,
            MinStock = 5,
            ImagePath = "/uploads/products/mouse.png",
            GalleryImagePaths = new[] { "/uploads/products/mouse-2.png" }
        });

        result.RequiresRestock.Should().BeTrue();
        result.ImagePath.Should().Be("/uploads/products/mouse.png");
        result.Category.Should().Be("Perifericos");
        result.Brand.Should().Be("ComfortLine");
        result.GalleryImages.Should().ContainSingle();
        createdProduct.Should().NotBeNull();
        createdProduct!.RequiresRestock.Should().BeTrue();
        createdProduct.ImagePath.Should().Be("/uploads/products/mouse.png");
        createdProduct.GalleryImages.Should().ContainSingle(x => x.ImagePath == "/uploads/products/mouse-2.png");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenProductNameAlreadyExists()
    {
        var repository = new Mock<IProductRepository>();
        repository
            .Setup(x => x.GetActiveByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Name = "Mouse Corporativo", Price = 25m });

        var currentUser = new Mock<ICurrentUserService>();
        currentUser
            .Setup(x => x.GetCurrent())
            .Returns(new CurrentRequestContext { UserName = "admin" });

        var categoryRepository = new Mock<IProductCategoryRepository>();
        var saleRepository = new Mock<ISaleRepository>();
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new ProductService(repository.Object, categoryRepository.Object, saleRepository.Object, currentUser.Object, dateTimeProvider.Object, unitOfWork.Object);

        var action = async () => await service.CreateAsync(new CreateProductRequest
        {
            Name = "Mouse Corporativo",
            Category = "Perifericos",
            Brand = "ComfortLine",
            Price = 25m,
            Stock = 5,
            MinStock = 1
        });

        await action.Should().ThrowAsync<ConflictException>();
    }
}
