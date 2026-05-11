using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Account;

/// <summary>
/// Centraliza o login do painel administrativo e cria a sessão reaproveitada pela camada LMT.
/// </summary>
public sealed class LoginModel(IAuthService authService, ITenantsService tenantsService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Dashboard/Index");

        if (TempData.TryGetValue(nameof(ErrorMessage), out var value))
            ErrorMessage = value?.ToString();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        LoginResponseDto? response;

        try
        {
            response = await authService.POSTApiAuthLoginAsync(new LoginRequestDto
            {
                TenantSlug = Input.TenantSlug?.Trim(),
                Email = Input.Email?.Trim(),
                Password = Input.Password
            }, cancellationToken);
        }
        catch
        {
            ErrorMessage = "Não foi possível concluir o login agora. Revise os dados e tente novamente em instantes.";
            return Page();
        }

        if (response?.AccessToken is null || response.User?.Id is null || response.Tenant?.Id is null)
        {
            ErrorMessage = "Não foi possível autenticar com os dados informados.";
            return Page();
        }

        var role = DashboardRoles.FromApiValue(response.User.Role);

        // O painel web nasce voltado para a operação da academia. Alunos usam o aplicativo.
        if (role == DashboardRoles.Student || string.IsNullOrWhiteSpace(role))
        {
            ErrorMessage = "Este painel administrativo é exclusivo para administradores e professores da academia.";
            return Page();
        }

        var sessionState = new DashboardSessionState
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            UserId = response.User.Id,
            Name = response.User.Name,
            Email = response.User.Email,
            Username = response.User.Username,
            ProfilePhotoUrl = response.User.ProfilePhotoUrl,
            Role = role,
            TenantId = response.Tenant.Id,
            TenantSlug = response.Tenant.Slug,
            TenantName = response.Tenant.Name,
            TenantLogoUrl = response.Tenant.LogoUrl,
            PrimaryColor = response.Tenant.PrimaryColor,
            SecondaryColor = response.Tenant.SecondaryColor,
            // A camada de aplicação já expõe ExpiresAt como DateTimeOffset? pronto para uso no painel.
            ExpiresAtUtc = response.ExpiresAt ?? DateTimeOffset.UtcNow.AddHours(8)
        };

        // O token entra primeiro na sessão para que a camada LMT consiga autenticar a chamada seguinte
        // que recupera os módulos contratados pela academia.
        HttpContext.Session.SetDashboardSession(sessionState);
        sessionState.EnabledFeatureCodes = await LoadEnabledFeatureCodesAsync(cancellationToken);
        HttpContext.Session.SetDashboardSession(sessionState);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, response.User.Id.Value.ToString()),
            new(ClaimTypes.Name, response.User.Name ?? response.User.Email ?? "Usuário"),
            new(ClaimTypes.Email, response.User.Email ?? string.Empty),
            new(ClaimTypes.Role, role),
            new("tenant_id", response.Tenant.Id.Value.ToString()),
            new("tenant_slug", response.Tenant.Slug ?? string.Empty)
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
                ExpiresUtc = sessionState.ExpiresAtUtc ?? DateTimeOffset.UtcNow.AddHours(8)
            });

        return RedirectToPage("/Dashboard/Index");
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Informe o nome da academia.")]
        [Display(Name = "Nome da academia")]
        public string? TenantSlug { get; set; }

        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Informe a senha.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string? Password { get; set; }
    }

    /// <summary>
    /// Carrega as features habilitadas para a academia atual.
    /// Em caso de falha, o painel continua navegável apenas com as áreas centrais.
    /// </summary>
    private async Task<List<string>> LoadEnabledFeatureCodesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var featuresResponse = await tenantsService.GETApiTenantsCurrentFeaturesAsync(cancellationToken);

            return featuresResponse?.Features?
                .Select(feature => feature.Name?.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Cast<string>()
                .ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }
}
