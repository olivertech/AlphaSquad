using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AlphaSquad.Backoffice.Security;

using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Account;

public sealed class LogoutModel(IBackofficeAuthService authService) : PageModel
{
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await authService.LogoutAsync(cancellationToken);
        }
        catch
        {
            // O objetivo principal ainda e limpar a sessao local do browser.
        }

        HttpContext.Session.ClearBackofficeSession();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Account/Login");
    }
}
