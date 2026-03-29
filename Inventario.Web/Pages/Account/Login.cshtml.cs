using System.ComponentModel.DataAnnotations;
using Inventario.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Account;

[
    AllowAnonymous
]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? PageErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, false, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                PageErrorMessage = "Credenciales inválidas. Verifica tu usuario y contraseña e intenta nuevamente.";
                ModelState.AddModelError(string.Empty, PageErrorMessage);
                return Page();
            }

            TempData["SuccessMessage"] = "Sesión iniciada correctamente.";
            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Dashboard/Index");
        }
        catch (Exception)
        {
            PageErrorMessage = "No fue posible iniciar sesión en este momento. Intenta nuevamente.";
            ModelState.AddModelError(string.Empty, PageErrorMessage);
            return Page();
        }
    }

    public class InputModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(100, ErrorMessage = "El usuario no puede superar los 100 caracteres.")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }
}
