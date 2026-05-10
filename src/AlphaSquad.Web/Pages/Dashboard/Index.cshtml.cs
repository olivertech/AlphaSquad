using AlphaSquad.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Dashboard;

/// <summary>
/// Home inicial do dashboard. Nesta primeira fase ela confirma a sessao e apresenta a base administrativa do produto.
/// </summary>
[Authorize(Policy = "DashboardAccess")]
public sealed class IndexModel : PageModel
{
    public DashboardSessionState? SessionState { get; private set; }

    public IActionResult OnGet()
    {
        SessionState = HttpContext.Session.GetDashboardSession();

        // O cookie sozinho nao e suficiente; se a sessao do token sumir, o dashboard precisa voltar ao login.
        if (SessionState?.AccessToken is null)
            return RedirectToPage("/Account/Login");

        return Page();
    }
}
