using AlphaSquad.Backoffice.Models;
using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Tenants;

/// <summary>
/// Lista consolidada das academias cadastradas na plataforma.
/// </summary>
public sealed class IndexModel(IBackofficeTenantWorkspaceService tenantWorkspaceService) : BackofficePageModelBase
{
    public IReadOnlyList<BackofficeTenantListItemViewModel> Tenants { get; private set; } = [];

    public int TotalTenants { get; private set; }
    public int ActiveTenants { get; private set; }
    public int InactiveTenants { get; private set; }
    public int PendingPasswordChanges { get; private set; }

    public async Task<IActionResult> OnGetAsync(bool? created, bool? updated, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        var tenants = await tenantWorkspaceService.ListAsync(cancellationToken);
        Tenants = tenants.Select(MapTenant).ToList();

        TotalTenants = Tenants.Count;
        ActiveTenants = Tenants.Count(item => item.IsActive);
        InactiveTenants = Tenants.Count(item => !item.IsActive);
        PendingPasswordChanges = tenants.Count(item => item.MustChangePassword);

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
