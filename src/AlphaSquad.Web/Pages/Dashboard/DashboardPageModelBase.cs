using AlphaSquad.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Dashboard;

/// <summary>
/// Base comum das páginas do dashboard.
/// Ela garante que a sessão HTTP ainda tenha o token necessário para consumir a API.
/// </summary>
[Authorize(Policy = "DashboardAccess")]
public abstract class DashboardPageModelBase : PageModel
{
    public DashboardSessionState? SessionState { get; protected set; }

    protected IActionResult PageOrLogin()
    {
        SessionState = HttpContext.Session.GetDashboardSession();

        // O cookie do painel pode continuar válido mesmo após a sessão expirar.
        // Quando isso acontecer, a tela volta ao login para reconstruir o contexto da API.
        if (SessionState?.AccessToken is null)
            return RedirectToPage("/Account/Login");

        return Page();
    }
}
