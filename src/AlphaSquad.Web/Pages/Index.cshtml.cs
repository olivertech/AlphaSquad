using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages;

/// <summary>
/// A raiz do projeto apenas redireciona o usuario para o fluxo correto do dashboard.
/// </summary>
public sealed class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true
            ? RedirectToPage("/Dashboard/Index")
            : RedirectToPage("/Account/Login");
    }
}
