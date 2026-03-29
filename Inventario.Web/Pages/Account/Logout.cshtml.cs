using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<Inventario.Infrastructure.Identity.ApplicationUser> _signInManager;

    public LogoutModel(SignInManager<Inventario.Infrastructure.Identity.ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "Sesion cerrada correctamente.";
        return RedirectToPage("/Account/Login");
    }
}
