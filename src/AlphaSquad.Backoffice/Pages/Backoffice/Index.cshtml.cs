using AlphaSquad.Backoffice.Models;
using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Backoffice;

/// <summary>
/// Dashboard inicial do sponsor com indicadores simples e uma visao consolidada da base de academias.
/// </summary>
public sealed class IndexModel(IBackofficeTenantWorkspaceService tenantWorkspaceService) : BackofficePageModelBase
{
    public IReadOnlyList<BackofficeTenantListItemViewModel> RecentTenants { get; private set; } = [];

    public int TotalTenants { get; private set; }
    public int ActiveTenants { get; private set; }
    public int PendingPasswordChanges { get; private set; }
    public int TotalContractedModules { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        var tenants = await tenantWorkspaceService.ListAsync(cancellationToken);
        var mapped = tenants
            .Select(MapTenant)
            .ToList();

        TotalTenants = mapped.Count;
        ActiveTenants = mapped.Count(item => item.IsActive);
        PendingPasswordChanges = tenants.Count(item => item.MustChangePassword);
        TotalContractedModules = tenants.Sum(item => item.FeatureCount);
        RecentTenants = mapped.Take(6).ToList();

        return Page();
    }

    private static BackofficeTenantListItemViewModel MapTenant(BackofficeTenantWorkspaceItem item) =>
        new()
        {
            Id = item.Id,
            Name = item.Name,
            Slug = item.Slug,
            IsActive = item.IsActive,
            StatusLabel = item.IsActive ? "Ativa" : "Inativa",
            PrimaryColor = item.PrimaryColor,
            SecondaryColor = item.SecondaryColor,
            LogoUrl = item.LogoUrl,
            PrimaryAdminName = item.PrimaryAdminName,
            PrimaryAdminEmail = item.PrimaryAdminEmail,
            FeatureCount = item.FeatureCount,
            CreatedAt = item.CreatedAt
        };
}
