namespace AlphaSquad.Backoffice.Pages;

/// <summary>
/// A raiz do projeto redireciona o sponsor para o fluxo correto do backoffice.
/// </summary>
public sealed class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true
            ? RedirectToPage("/Backoffice/Index")
            : RedirectToPage("/Account/Login");
    }
}
