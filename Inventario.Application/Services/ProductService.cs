using Inventario.Application.Abstractions;
using Inventario.Application.Abstractions.Repositories;
using Inventario.Application.Abstractions.Services;
using Inventario.Application.Common;
using Inventario.Application.Common.Exceptions;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Domain.Entities;

namespace Inventario.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepository,
        IProductCategoryRepository productCategoryRepository,
        ISaleRepository saleRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _saleRepository = saleRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _productRepository.GetActiveByNameAsync(request.Name.Trim(), cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("Ya existe un producto activo con ese nombre.");
        }

        await EnsureCategoryExistsAsync(request.Category, cancellationToken);

        var context = _currentUserService.GetCurrent();
        var product = new Product
        {
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Brand = request.Brand.Trim(),
            ShortDescription = string.IsNullOrWhiteSpace(request.ShortDescription) ? null : request.ShortDescription.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Price = request.Price,
            ImagePath = request.ImagePath,
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedBy = context.UserName ?? "system"
        };

        product.ConfigureInventory(request.Stock, request.MinStock);
        product.ReplaceGalleryImages(request.GalleryImagePaths, context.UserName ?? "system", _dateTimeProvider.UtcNow);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("El producto solicitado no fue encontrado.");

        var context = _currentUserService.GetCurrent();
        product.IsActive = false;
        product.UpdatedAt = _dateTimeProvider.UtcNow;
        product.UpdatedBy = context.UserName ?? "system";

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductOptionDto>> GetActiveOptionsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllActiveAsync(cancellationToken);
        return products.Select(x => new ProductOptionDto
        {
            Id = x.Id,
            Name = x.Name,
            Category = x.Category,
            Brand = x.Brand,
            Price = x.Price,
            Stock = x.Stock
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductRecommendationDto>> GetRelatedAsync(Guid productId, int take = 4, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("El producto solicitado no fue encontrado.");

        var related = await _productRepository.GetRelatedByCategoryAsync(product.Category, productId, take, cancellationToken);
        return related.Select(productItem => new ProductRecommendationDto
        {
            Id = productItem.Id,
            Name = productItem.Name,
            Category = productItem.Category,
            Brand = productItem.Brand,
            Price = productItem.Price,
            ImagePath = productItem.ImagePath,
            ShortDescription = productItem.ShortDescription
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductRecommendationDto>> GetTopSellingAsync(Guid? excludedProductId = null, int take = 4, CancellationToken cancellationToken = default)
    {
        var topSelling = await _saleRepository.GetTopSellingProductIdsAsync(take, excludedProductId, cancellationToken);
        if (topSelling.Count == 0)
        {
            return Array.Empty<ProductRecommendationDto>();
        }

        var products = await _productRepository.GetByIdsAsync(topSelling.Select(x => x.ProductId).ToArray(), cancellationToken);
        var productLookup = products.ToDictionary(x => x.Id);

        return topSelling
            .Where(item => productLookup.ContainsKey(item.ProductId))
            .Select(item =>
            {
                var product = productLookup[item.ProductId];
                return new ProductRecommendationDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    Brand = product.Brand,
                    Price = product.Price,
                    ImagePath = product.ImagePath,
                    ShortDescription = product.ShortDescription,
                    SoldQuantity = item.Quantity
                };
            })
            .ToArray();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<IReadOnlyCollection<ProductDto>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetLowStockAsync(cancellationToken);
        return products.Select(Map).ToArray();
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryRequest request, CancellationToken cancellationToken = default)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

        var normalizedRequest = new ProductQueryRequest
        {
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim(),
            StockStatus = string.IsNullOrWhiteSpace(request.StockStatus) ? null : request.StockStatus.Trim(),
            SortBy = string.IsNullOrWhiteSpace(request.SortBy) ? null : request.SortBy.Trim(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _productRepository.GetPagedAsync(normalizedRequest, cancellationToken);
        return new PagedResult<ProductDto>
        {
            Items = result.Items.Select(Map).ToArray(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("El producto solicitado no fue encontrado.");

        var existing = await _productRepository.GetActiveByNameAsync(request.Name.Trim(), cancellationToken);
        if (existing is not null && existing.Id != id)
        {
            throw new ConflictException("Ya existe un producto activo con ese nombre.");
        }

        await EnsureCategoryExistsAsync(request.Category, cancellationToken);

        var context = _currentUserService.GetCurrent();
        product.Name = request.Name.Trim();
        product.Category = request.Category.Trim();
        product.Brand = request.Brand.Trim();
        product.ShortDescription = string.IsNullOrWhiteSpace(request.ShortDescription) ? null : request.ShortDescription.Trim();
        product.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        product.Price = request.Price;
        product.ImagePath = request.ImagePath;
        product.ConfigureInventory(request.Stock, request.MinStock);
        product.ReplaceGalleryImages(request.GalleryImagePaths, context.UserName ?? "system", _dateTimeProvider.UtcNow);
        product.UpdatedAt = _dateTimeProvider.UtcNow;
        product.UpdatedBy = context.UserName ?? "system";

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    private static ProductDto Map(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Category = product.Category,
        Brand = product.Brand,
        ShortDescription = product.ShortDescription,
        Description = product.Description,
        Price = product.Price,
        ImagePath = product.ImagePath,
        GalleryImages = product.GalleryImages
            .OrderBy(image => image.SortOrder)
            .Select(image => new ProductGalleryImageDto
            {
                Id = image.Id,
                ImagePath = image.ImagePath,
                SortOrder = image.SortOrder
            })
            .ToArray(),
        Stock = product.Stock,
        MinStock = product.MinStock,
        RequiresRestock = product.RequiresRestock,
        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };

    private async Task EnsureCategoryExistsAsync(string categoryName, CancellationToken cancellationToken)
    {
        var category = await _productCategoryRepository.GetByNameAsync(categoryName.Trim(), cancellationToken);
        if (category is null || !category.IsActive)
        {
            throw new BusinessRuleException("La categoría seleccionada no está disponible en el catálogo maestro.");
        }
    }
}
