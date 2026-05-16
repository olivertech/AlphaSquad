using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AlphaSquad.Backoffice.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace AlphaSquad.Backoffice.Pages.Account;

/// <summary>
/// Centraliza o login do sponsor no backoffice enquanto a autenticacao master do backend ainda nao foi criada.
/// </summary>
public sealed class LoginModel(IOptions<BackofficeBootstrapOptions> bootstrapOptions) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Backoffice/Index");

        if (TempData.TryGetValue(nameof(ErrorMessage), out var value))
            ErrorMessage = value?.ToString();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var options = bootstrapOptions.Value;
        var normalizedEmail = Input.Email?.Trim().ToLowerInvariant();

        if (!string.Equals(normalizedEmail, options.Email.Trim().ToLowerInvariant(), StringComparison.Ordinal) ||
            !string.Equals(Input.Password, options.Password, StringComparison.Ordinal))
        {
            ErrorMessage = "Nao foi possivel autenticar com as credenciais informadas.";
            return Page();
        }

        var sessionState = new BackofficeSessionState
        {
            UserId = Guid.NewGuid(),
            Name = options.Name,
            Email = options.Email,
            Role = BackofficeRoles.Owner,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        HttpContext.Session.SetBackofficeSession(sessionState);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sessionState.UserId.Value.ToString()),
            new(ClaimTypes.Name, options.Name),
            new(ClaimTypes.Email, options.Email),
            new(ClaimTypes.Role, BackofficeRoles.Owner)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = sessionState.ExpiresAtUtc
            });

        return RedirectToPage("/Backoffice/Index");
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
