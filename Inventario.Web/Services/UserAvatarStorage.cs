using Inventario.Application.Common.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Inventario.Web.Services;

public sealed class UserAvatarStorage : IUserAvatarStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxFileSizeBytes = 2 * 1024 * 1024;
    private const string UploadsFolder = "uploads/users/avatars";
    private readonly IWebHostEnvironment _environment;

    public UserAvatarStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public Task<string?> SaveAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        return ReplaceAsync(null, file, removeCurrentAvatar: false, cancellationToken);
    }

    public async Task<string?> ReplaceAsync(string? currentAvatarPath, IFormFile? newFile, bool removeCurrentAvatar, CancellationToken cancellationToken = default)
    {
        if (removeCurrentAvatar)
        {
            DeletePhysicalFile(currentAvatarPath);
            currentAvatarPath = null;
        }

        if (newFile is null || newFile.Length == 0)
        {
            return currentAvatarPath;
        }

        ValidateFile(newFile);

        var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var uploadDirectory = Path.Combine(webRootPath, "uploads", "users", "avatars");
        Directory.CreateDirectory(uploadDirectory);

        var extension = Path.GetExtension(newFile.FileName);
        var fileName = $"user-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var absolutePath = Path.Combine(uploadDirectory, fileName);

        await using (var stream = File.Create(absolutePath))
        {
            await newFile.CopyToAsync(stream, cancellationToken);
        }

        DeletePhysicalFile(currentAvatarPath);
        return $"/{UploadsFolder}/{fileName}";
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
        {
            throw new BusinessRuleException("La imagen de perfil no puede superar los 2 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new BusinessRuleException("La imagen de perfil debe estar en formato JPG, PNG o WEBP.");
        }
    }

    private void DeletePhysicalFile(string? avatarPath)
    {
        if (string.IsNullOrWhiteSpace(avatarPath))
        {
            return;
        }

        var relativePath = avatarPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var absolutePath = Path.Combine(webRootPath, relativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }
}
