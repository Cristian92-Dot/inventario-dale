using Microsoft.AspNetCore.Http;

namespace Inventario.Web.Services;

public interface IProductImageStorage
{
    Task<string?> SaveAsync(IFormFile? file, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> SaveManyAsync(IEnumerable<IFormFile>? files, CancellationToken cancellationToken = default);
    Task<string?> ReplaceAsync(string? currentImagePath, IFormFile? newFile, bool removeCurrentImage, CancellationToken cancellationToken = default);
    void DeleteMany(IEnumerable<string>? imagePaths);
}
