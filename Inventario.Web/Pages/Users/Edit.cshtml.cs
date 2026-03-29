using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Users.Dtos;
using Inventario.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Users;

[Authorize(Roles = "ADMIN")]
public class EditModel : PageModel
{
    private readonly IUserManagementService _userManagementService;
    private readonly IUserAvatarStorage _userAvatarStorage;

    public EditModel(IUserManagementService userManagementService, IUserAvatarStorage userAvatarStorage)
    {
        _userManagementService = userManagementService;
        _userAvatarStorage = userAvatarStorage;
    }

    [BindProperty]
    public UpdateUserProfileRequest Input { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    [BindProperty]
    public bool RemoveCurrentAvatar { get; set; }

    public string UserId { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        UserId = id;
        Input = new UpdateUserProfileRequest
        {
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarPath = user.AvatarPath
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken)
    {
        UserId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var existingUser = await _userManagementService.GetByIdAsync(id, cancellationToken);
            if (existingUser is null)
            {
                return NotFound();
            }

            Input.AvatarPath = await _userAvatarStorage.ReplaceAsync(existingUser.AvatarPath, AvatarFile, RemoveCurrentAvatar, cancellationToken);
            await _userManagementService.UpdateProfileAsync(id, Input, cancellationToken);
            TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
            return RedirectToPage("/Users/Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
