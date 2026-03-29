using Inventario.Application.Abstractions.Services;
using Inventario.Application.Features.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Users;

[Authorize(Roles = "ADMIN")]
public class IndexModel : PageModel
{
    private readonly IUserManagementService _userManagementService;

    public IndexModel(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public IReadOnlyCollection<UserManagementDto> Users { get; private set; } = Array.Empty<UserManagementDto>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Users = await _userManagementService.GetAllAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(string id, CancellationToken cancellationToken)
    {
        await _userManagementService.ToggleStatusAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Estado del usuario actualizado correctamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(string id, string role, CancellationToken cancellationToken)
    {
        await _userManagementService.UpdateRoleAsync(id, new UpdateUserRoleRequest { Role = role }, cancellationToken);
        TempData["SuccessMessage"] = "Rol actualizado correctamente.";
        return RedirectToPage();
    }
}
