using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Users.Dtos;
using Inventario.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Users;

[Authorize(Roles = "ADMIN")]
public class CreateModel : PageModel
{
    private readonly IUserManagementService _userManagementService;
    private readonly IUserAvatarStorage _userAvatarStorage;

    public CreateModel(IUserManagementService userManagementService, IUserAvatarStorage userAvatarStorage)
    {
        _userManagementService = userManagementService;
        _userAvatarStorage = userAvatarStorage;
    }

    [BindProperty]
    public CreateUserRequest Input { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            Input.AvatarPath = await _userAvatarStorage.SaveAsync(AvatarFile, cancellationToken);
            await _userManagementService.CreateAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Usuario registrado correctamente.";
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
