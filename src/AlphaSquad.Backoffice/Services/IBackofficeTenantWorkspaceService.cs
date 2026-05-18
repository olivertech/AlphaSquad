using AlphaSquad.Backoffice.Models;

namespace AlphaSquad.Backoffice.Services;

public interface IBackofficeTenantWorkspaceService
{
    Task<IReadOnlyList<BackofficeTenantWorkspaceItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<BackofficeTenantWorkspaceItem?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeTenantProvisioningResult> CreateAsync(BackofficeTenantCreateCommand command, CancellationToken cancellationToken = default);
    Task<BackofficeTenantWorkspaceItem?> UpdateAsync(Guid id, BackofficeTenantUpdateCommand command, CancellationToken cancellationToken = default);
    Task<BackofficeTenantWorkspaceItem> UploadLogoAsync(Guid id, IFormFile file, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BackofficeFeatureOptionViewModel>> GetFeatureCatalogAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BackofficeTenantAdminItem>> GetAdminsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<BackofficeTenantAdminProvisioningResult> CreateAdminAsync(Guid tenantId, BackofficeTenantAdminCreateCommand command, CancellationToken cancellationToken = default);
    Task<BackofficeTenantAdminProvisioningResult> ResetAdminPasswordAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BackofficeTenantAuditLogItem>> GetAuditAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
