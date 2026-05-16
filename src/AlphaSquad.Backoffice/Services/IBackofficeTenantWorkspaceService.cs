namespace AlphaSquad.Backoffice.Services;

public interface IBackofficeTenantWorkspaceService
{
    Task<IReadOnlyList<BackofficeTenantWorkspaceItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<BackofficeTenantWorkspaceItem?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeTenantWorkspaceItem> CreateAsync(BackofficeTenantCreateCommand command, CancellationToken cancellationToken = default);
    Task<BackofficeTenantWorkspaceItem?> UpdateAsync(Guid id, BackofficeTenantUpdateCommand command, CancellationToken cancellationToken = default);
}
