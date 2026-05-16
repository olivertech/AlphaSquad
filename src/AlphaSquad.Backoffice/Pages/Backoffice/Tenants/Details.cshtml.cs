using AlphaSquad.Backoffice.Models;
using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Tenants;

/// <summary>
/// Exibe o resumo completo da academia provisionada e da credencial inicial do administrador.
/// </summary>
public sealed class DetailsModel(IBackofficeTenantWorkspaceService tenantWorkspaceService) : BackofficePageModelBase
{
    public BackofficeTenantDetailsViewModel? Tenant { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, bool? created, bool? updated, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (created == true)
            ShowSuccessToast("Academia criada e pronta para iniciar operacao.", title: "Onboarding concluido");

        if (updated == true)
            ShowSuccessToast("Academia atualizada com sucesso.", title: "Dados salvos");

        var tenant = await tenantWorkspaceService.GetAsync(id, cancellationToken);
        if (tenant is null)
        {
            ShowWarningToast("A academia solicitada nao foi encontrada.", persist: true, title: "Cadastro inexistente");
            return RedirectToPage("/Backoffice/Tenants/Index");
        }

        Tenant = new BackofficeTenantDetailsViewModel
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            IsActive = tenant.IsActive,
            StatusLabel = tenant.IsActive ? "Ativa" : "Inativa",
            LogoUrl = tenant.LogoUrl,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            Features = BackofficeTenantCatalog.ResolveFeatures(tenant.FeatureCodes),
            PrimaryAdminName = tenant.PrimaryAdminName,
            PrimaryAdminEmail = tenant.PrimaryAdminEmail,
            TemporaryPassword = tenant.TemporaryPassword,
            MustChangePassword = tenant.MustChangePassword,
            CreatedAt = tenant.CreatedAt
        };

        return Page();
    }
}
