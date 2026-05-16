using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AlphaSquad.Backoffice.Security;
using AlphaSquad.Backoffice.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AlphaSquad.Backoffice.Pages.Account;

/// <summary>
/// Centraliza o login do sponsor no backoffice usando a API master da AlphaSquad.
/// </summary>
public sealed class LoginModel(IBackofficeAuthService authService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // O backoffice deve sempre abrir em estado deslogado quando acessado pela rota de login.
        HttpContext.Session.ClearBackofficeSession();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (TempData.TryGetValue(nameof(ErrorMessage), out var value))
            ErrorMessage = value?.ToString();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var response = await authService.LoginAsync(Input.Email?.Trim() ?? string.Empty, Input.Password ?? string.Empty, cancellationToken);

            var sessionState = new BackofficeSessionState
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                UserId = response.User.Id,
                Name = response.User.Name,
                Email = response.User.Email,
                Role = response.User.Role,
                MustChangePassword = response.User.MustChangePassword,
                ProfilePhotoUrl = response.User.ProfilePhotoUrl,
                ExpiresAtUtc = new DateTimeOffset(DateTime.SpecifyKind(response.ExpiresAt, DateTimeKind.Utc))
            };

            HttpContext.Session.SetBackofficeSession(sessionState);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, sessionState.UserId!.Value.ToString()),
                new(ClaimTypes.Name, sessionState.Name ?? string.Empty),
                new(ClaimTypes.Email, sessionState.Email ?? string.Empty),
                new(ClaimTypes.Role, sessionState.Role ?? BackofficeRoles.Owner)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true,
                    ExpiresUtc = sessionState.ExpiresAtUtc
                });

            return RedirectToPage("/Backoffice/Index");
        }
        catch (BackofficeApiException exception) when (exception.StatusCode is 400 or 401)
        {
            ErrorMessage = "Nao foi possivel autenticar com as credenciais informadas.";
            return Page();
        }
        catch
        {
            ErrorMessage = "Nao foi possivel conectar o backoffice a API master agora.";
            return Page();
        }
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Informe a senha.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string? Password { get; set; }
    }
}
