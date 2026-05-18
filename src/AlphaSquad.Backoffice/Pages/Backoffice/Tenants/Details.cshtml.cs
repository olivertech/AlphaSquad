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
            StatusLabel = tenant.IsActive ? "Academia Ativa" : "Academia Inativa",
            LogoUrl = tenant.LogoUrl,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            Features = tenant.Features.Count > 0
                ? tenant.Features
                : BackofficeTenantCatalog.ResolveFeatures(tenant.FeatureCodes),
            PrimaryAdminName = tenant.PrimaryAdminName,
            PrimaryAdminEmail = tenant.PrimaryAdminEmail,
            TemporaryPassword = TempData["Backoffice.NewTenantTemporaryPassword"]?.ToString() ?? string.Empty,
            MustChangePassword = bool.TryParse(TempData["Backoffice.NewTenantMustChangePassword"]?.ToString(), out var mustChangePassword)
                ? mustChangePassword
                : tenant.MustChangePassword,
            CreatedAt = tenant.CreatedAt,
            AdminUsers = tenant.AdminUsers,
            AuditLogs = tenant.AuditLogs
        };

        return Page();
    }
}
