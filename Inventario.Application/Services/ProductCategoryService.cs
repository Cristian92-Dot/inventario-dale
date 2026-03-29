using Inventario.Application.Abstractions;
using Inventario.Application.Abstractions.Repositories;
using Inventario.Application.Abstractions.Services;
using Inventario.Application.Common.Exceptions;
using Inventario.Application.Features.Categories.Dtos;
using Inventario.Domain.Entities;

namespace Inventario.Application.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ProductCategoryService(
        IProductCategoryRepository productCategoryRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _productCategoryRepository = productCategoryRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductCategoryDto> CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim();
        var existing = await _productCategoryRepository.GetByNameAsync(normalizedName, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("Ya existe una categoría con ese nombre.");
        }

        var context = _currentUserService.GetCurrent();
        var category = new ProductCategory
        {
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            SortOrder = await _productCategoryRepository.GetNextSortOrderAsync(cancellationToken),
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedBy = context.UserName ?? "system"
        };

        await _productCategoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<IReadOnlyCollection<ProductCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _productCategoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(Map).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductCategoryOptionDto>> GetActiveOptionsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _productCategoryRepository.GetActiveAsync(cancellationToken);
        return categories.Select(category => new ProductCategoryOptionDto
        {
            Id = category.Id,
            Name = category.Name
        }).ToArray();
    }

    public async Task ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _productCategoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("La categoría indicada no existe.");

        category.IsActive = !category.IsActive;
        category.UpdatedAt = _dateTimeProvider.UtcNow;
        category.UpdatedBy = _currentUserService.GetCurrent().UserName ?? "system";
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ProductCategoryDto Map(ProductCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        IsActive = category.IsActive,
        SortOrder = category.SortOrder
    };
}
