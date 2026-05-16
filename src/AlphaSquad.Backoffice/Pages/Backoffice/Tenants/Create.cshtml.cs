using AlphaSquad.Backoffice.Models;
using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Tenants;

/// <summary>
/// Tela de onboarding inicial de uma nova academia.
/// Agora o fluxo usa a API master real para provisionar tenant, features e admin inicial.
/// </summary>
public sealed class CreateModel(IBackofficeTenantWorkspaceService tenantWorkspaceService) : BackofficePageModelBase
{
    [BindProperty]
    public BackofficeTenantFormInputModel Input { get; set; } = new();

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

    public IReadOnlyList<BackofficeFeatureOptionViewModel> FeatureOptions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        FeatureOptions = await tenantWorkspaceService.GetFeatureCatalogAsync(cancellationToken);
        if (Input.FeatureCodes.Count == 0)
            Input.FeatureCodes = [.. FeatureOptions.Take(4).Select(feature => feature.Code)];

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        FeatureOptions = await tenantWorkspaceService.GetFeatureCatalogAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var provisioning = await tenantWorkspaceService.CreateAsync(new BackofficeTenantCreateCommand
            {
                Name = Input.Name,
                Slug = Input.Slug,
                PrimaryColor = NormalizeColor(Input.PrimaryColor),
                SecondaryColor = NormalizeColor(Input.SecondaryColor),
                IsActive = Input.IsActive,
                FeatureCodes = Input.FeatureCodes,
                PrimaryAdminName = Input.AdminName,
                PrimaryAdminEmail = Input.AdminEmail,
                LogoUrl = null
            }, cancellationToken);

            if (LogoFile is not null && LogoFile.Length > 0)
                await tenantWorkspaceService.UploadLogoAsync(provisioning.Tenant.Id, LogoFile, cancellationToken);

            TempData["Backoffice.NewTenantTemporaryPassword"] = provisioning.TemporaryPassword;
            TempData["Backoffice.NewTenantMustChangePassword"] = provisioning.MustChangePassword.ToString();
            ShowSuccessToast("Academia criada com sucesso.", persist: true, title: "Onboarding iniciado");
            return RedirectToPage("/Backoffice/Tenants/Details", new { id = provisioning.Tenant.Id, created = true });
        }
        catch (BackofficeApiException ex)
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
}
