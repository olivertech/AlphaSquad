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
    private const string ToastMessageKey = "Dashboard.Toast.Message";
    private const string ToastTypeKey = "Dashboard.Toast.Type";
    private const string ToastTitleKey = "Dashboard.Toast.Title";

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

    protected void ShowSuccessToast(string message, bool persist = false, string? title = null) =>
        SetToast("success", message, persist, title);

    protected void ShowErrorToast(string message, bool persist = false, string? title = null) =>
        SetToast("error", message, persist, title);

    protected void ShowWarningToast(string message, bool persist = false, string? title = null) =>
        SetToast("warning", message, persist, title);

    private void SetToast(string type, string message, bool persist, string? title)
    {
        if (persist)
        {
            TempData[ToastTypeKey] = type;
            TempData[ToastMessageKey] = message;

            if (!string.IsNullOrWhiteSpace(title))
                TempData[ToastTitleKey] = title;
            else
                TempData.Remove(ToastTitleKey);

            return;
        }

        ViewData[ToastTypeKey] = type;
        ViewData[ToastMessageKey] = message;

        if (!string.IsNullOrWhiteSpace(title))
            ViewData[ToastTitleKey] = title;
        else
            ViewData.Remove(ToastTitleKey);
    }
}
