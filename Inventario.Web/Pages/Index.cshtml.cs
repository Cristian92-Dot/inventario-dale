using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        return RedirectToPage("/Account/Login");
    }
}
