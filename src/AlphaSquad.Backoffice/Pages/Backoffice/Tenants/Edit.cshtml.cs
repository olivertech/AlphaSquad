using AlphaSquad.Backoffice.Models;
using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Tenants;

/// <summary>
/// Ajusta dados basicos da academia usando o backend master real.
/// </summary>
public sealed class EditModel(IBackofficeTenantWorkspaceService tenantWorkspaceService) : BackofficePageModelBase
{
    [BindProperty]
    public BackofficeTenantFormInputModel Input { get; set; } = new();

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

    [BindProperty]
    public string? CurrentLogoUrl { get; set; }

    public Guid TenantId { get; private set; }
    public IReadOnlyList<BackofficeFeatureOptionViewModel> FeatureOptions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        FeatureOptions = await tenantWorkspaceService.GetFeatureCatalogAsync(cancellationToken);
        var tenant = await tenantWorkspaceService.GetAsync(id, cancellationToken);
        if (tenant is null)
        {
            ShowWarningToast("A academia solicitada nao foi encontrada.", persist: true, title: "Cadastro inexistente");
            return RedirectToPage("/Backoffice/Tenants/Index");
        }

        TenantId = tenant.Id;
        CurrentLogoUrl = tenant.LogoUrl;
        Input = new BackofficeTenantFormInputModel
        {
            Name = tenant.Name,
            Slug = tenant.Slug,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            IsActive = tenant.IsActive,
            FeatureCodes = [.. tenant.FeatureCodes],
            AdminName = tenant.PrimaryAdminName,
            AdminEmail = tenant.PrimaryAdminEmail
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        TenantId = id;
        FeatureOptions = await tenantWorkspaceService.GetFeatureCatalogAsync(cancellationToken);
        CurrentLogoUrl ??= string.Empty;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var tenant = await tenantWorkspaceService.UpdateAsync(id, new BackofficeTenantUpdateCommand
            {
                Name = Input.Name,
                Slug = Input.Slug,
                PrimaryColor = NormalizeColor(Input.PrimaryColor),
                SecondaryColor = NormalizeColor(Input.SecondaryColor),
                IsActive = Input.IsActive,
                FeatureCodes = Input.FeatureCodes,
                AdminName = Input.AdminName,
                AdminEmail = Input.AdminEmail,
                LogoUrl = null
            }, cancellationToken);

            if (tenant is null)
            {
                ShowWarningToast("A academia solicitada nao foi encontrada.", persist: true, title: "Cadastro inexistente");
                return RedirectToPage("/Backoffice/Tenants/Index");
            }

            if (LogoFile is not null && LogoFile.Length > 0)
                tenant = await tenantWorkspaceService.UploadLogoAsync(id, LogoFile, cancellationToken);

            ShowSuccessToast("Academia atualizada com sucesso.", persist: true, title: "Dados salvos");
            return RedirectToPage("/Backoffice/Tenants/Details", new { id = tenant.Id, updated = true });
        }
        catch (BackofficeApiException ex)
        {
            ShowWarningToast(ex.Message, title: "Nao foi possivel salvar");
            return Page();
        }
        catch
        {
            ShowErrorToast("Nao foi possivel atualizar esta academia agora. Tente novamente em instantes.", title: "Falha na atualizacao");
            return Page();
        }
    }

    private static string NormalizeColor(string color)
    {
        var normalized = color.Trim();
        return normalized.StartsWith('#') ? normalized : $"#{normalized}";
    }
}
