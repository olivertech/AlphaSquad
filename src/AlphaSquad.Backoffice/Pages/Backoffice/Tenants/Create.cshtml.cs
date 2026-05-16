using System.Globalization;
using AlphaSquad.Backoffice.Models;
using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Tenants;

/// <summary>
/// Tela de onboarding inicial de uma nova academia.
/// Nesta fase, a gravacao acontece em memoria apenas para validar o fluxo do sponsor.
/// </summary>
public sealed class CreateModel(IBackofficeTenantWorkspaceService tenantWorkspaceService) : BackofficePageModelBase
{
    [BindProperty]
    public BackofficeTenantFormInputModel Input { get; set; } = new()
    {
        FeatureCodes = [.. BackofficeTenantCatalog.FeatureOptions.Take(4).Select(feature => feature.Code)]
    };

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

    public IReadOnlyList<BackofficeFeatureOptionViewModel> FeatureOptions => BackofficeTenantCatalog.FeatureOptions;

    public IActionResult OnGet()
    {
        var result = PageOrLogin();
        return result is PageResult ? Page() : result;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var logoUrl = await ConvertLogoToDataUrlAsync(LogoFile, cancellationToken);

            var tenant = await tenantWorkspaceService.CreateAsync(new BackofficeTenantCreateCommand
            {
                Name = Input.Name,
                Slug = Input.Slug,
                PrimaryColor = NormalizeColor(Input.PrimaryColor),
                SecondaryColor = NormalizeColor(Input.SecondaryColor),
                IsActive = Input.IsActive,
                FeatureCodes = Input.FeatureCodes,
                PrimaryAdminName = Input.AdminName,
                PrimaryAdminEmail = Input.AdminEmail,
                LogoUrl = logoUrl
            }, cancellationToken);

            ShowSuccessToast($"Academia criada com sucesso. Senha provisoria gerada: {tenant.TemporaryPassword}", persist: true, title: "Onboarding iniciado");
            return RedirectToPage("/Backoffice/Tenants/Details", new { id = tenant.Id, created = true });
        }
        catch (InvalidOperationException ex)
        {
            ShowWarningToast(ex.Message, title: "Nao foi possivel concluir");
            return Page();
        }
        catch
        {
            ShowErrorToast("Nao foi possivel criar a nova academia agora. Tente novamente em instantes.", title: "Falha no onboarding");
            return Page();
        }
    }

    private static string NormalizeColor(string color)
    {
        var normalized = color.Trim();
        return normalized.StartsWith('#') ? normalized : $"#{normalized}";
    }

    private static async Task<string?> ConvertLogoToDataUrlAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return null;

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        return $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
    }
}
