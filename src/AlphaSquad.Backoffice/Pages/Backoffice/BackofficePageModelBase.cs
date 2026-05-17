using AlphaSquad.Backoffice.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace AlphaSquad.Backoffice.Pages.Backoffice;

/// <summary>
/// Base comum das paginas do backoffice.
/// Ela garante que a sessao HTTP ainda tenha o contexto do sponsor autenticado.
/// </summary>
[Authorize(Policy = "BackofficeAccess")]
public abstract class BackofficePageModelBase : PageModel
{
    private const string ToastMessageKey = "Backoffice.Toast.Message";
    private const string ToastTypeKey = "Backoffice.Toast.Type";
    private const string ToastTitleKey = "Backoffice.Toast.Title";

    public BackofficeSessionState? SessionState { get; protected set; }

    protected IActionResult PageOrLogin()
    {
        SessionState = HttpContext.Session.GetBackofficeSession();

        if (SessionState is null && User.Identity?.IsAuthenticated == true)
        {
            var authenticationResult = HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                .GetAwaiter()
                .GetResult();

            var principal = authenticationResult.Principal ?? User;
            var accessToken = authenticationResult.Properties?.GetTokenValue("access_token");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var expiresAtRaw = authenticationResult.Properties?.GetTokenValue("expires_at");
                var refreshToken = authenticationResult.Properties?.GetTokenValue("refresh_token");
                var mustChangePasswordRaw = principal.FindFirstValue("backoffice_must_change_password");

                SessionState = new BackofficeSessionState
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserId = Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null,
                    Name = principal.FindFirstValue(ClaimTypes.Name),
                    Email = principal.FindFirstValue(ClaimTypes.Email),
                    Role = principal.FindFirstValue(ClaimTypes.Role),
                    MustChangePassword = bool.TryParse(mustChangePasswordRaw, out var mustChangePassword) && mustChangePassword,
                    ProfilePhotoUrl = principal.FindFirstValue("backoffice_profile_photo_url"),
                    ExpiresAtUtc = DateTimeOffset.TryParse(expiresAtRaw, out var expiresAt)
                        ? expiresAt
                        : DateTimeOffset.UtcNow.AddHours(8)
                };

                HttpContext.Session.SetBackofficeSession(SessionState);
            }
        }

        if (SessionState is null)
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
