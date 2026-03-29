using Microsoft.AspNetCore.Http;

namespace Inventario.Web.Services;

public interface IUserAvatarStorage
{
    Task<string?> SaveAsync(IFormFile? file, CancellationToken cancellationToken = default);
    Task<string?> ReplaceAsync(string? currentAvatarPath, IFormFile? newFile, bool removeCurrentAvatar, CancellationToken cancellationToken = default);
}
