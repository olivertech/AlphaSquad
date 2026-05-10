using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Account;

/// <summary>
/// Encerra a sessao do dashboard e tenta revogar o refresh token do lado da API.
/// </summary>
public sealed class LogoutModel(IAuthService authService) : PageModel
{
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await authService.POSTApiAuthLogoutAsync(cancellationToken);
        }
        catch
        {
            // Mesmo se a API ja nao aceitar o token, o dashboard precisa limpar sua sessao local.
        }

        HttpContext.Session.ClearDashboardSession();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Account/Login");
    }
}
