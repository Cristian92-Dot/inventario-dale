using Inventario.Application.Common.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Inventario.Web.Services;

public sealed class ProductImageStorage : IProductImageStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxFileSizeBytes = 2 * 1024 * 1024;
    private const string UploadsFolder = "uploads/products";
    private readonly IWebHostEnvironment _environment;

    public ProductImageStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public Task<string?> SaveAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        return ReplaceAsync(null, file, removeCurrentImage: false, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> SaveManyAsync(IEnumerable<IFormFile>? files, CancellationToken cancellationToken = default)
    {
        if (files is null)
        {
            return Array.Empty<string>();
        }

        var result = new List<string>();
        foreach (var file in files.Where(file => file is not null && file.Length > 0))
        {
            var savedPath = await SaveAsync(file, cancellationToken);
            if (!string.IsNullOrWhiteSpace(savedPath))
            {
                result.Add(savedPath);
            }
        }

        return result;
    }

    public async Task<string?> ReplaceAsync(string? currentImagePath, IFormFile? newFile, bool removeCurrentImage, CancellationToken cancellationToken = default)
    {
        if (removeCurrentImage)
        {
            DeletePhysicalFile(currentImagePath);
            currentImagePath = null;
        }

        if (newFile is null || newFile.Length == 0)
        {
            return currentImagePath;
        }

        ValidateFile(newFile);

        var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var uploadsDirectory = Path.Combine(webRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsDirectory);

        var extension = Path.GetExtension(newFile.FileName);
        var fileName = $"product-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var absolutePath = Path.Combine(uploadsDirectory, fileName);

        await using (var stream = File.Create(absolutePath))
        {
            await newFile.CopyToAsync(stream, cancellationToken);
        }

        DeletePhysicalFile(currentImagePath);
        return $"/{UploadsFolder}/{fileName}";
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
        {
            throw new BusinessRuleException("La imagen del producto no puede superar los 2 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new BusinessRuleException("La imagen debe estar en formato JPG, PNG o WEBP.");
        }
    }

    private void DeletePhysicalFile(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var absolutePath = Path.Combine(webRootPath, relativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    public void DeleteMany(IEnumerable<string>? imagePaths)
    {
        if (imagePaths is null)
        {
            return;
        }

        foreach (var imagePath in imagePaths)
        {
            DeletePhysicalFile(imagePath);
        }
    }
}
