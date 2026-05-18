using AlphaSquad.Backoffice.Models;

namespace AlphaSquad.Backoffice.Services;

/// <summary>
/// Modelo de trabalho do backoffice para representar academias provisionadas pela AlphaSquad.
/// </summary>
public sealed class BackofficeTenantWorkspaceItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#2563eb";
    public string SecondaryColor { get; set; } = "#14b8a6";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int FeatureCount { get; set; }
    public List<string> FeatureCodes { get; set; } = [];
    public IReadOnlyList<BackofficeFeatureOptionViewModel> Features { get; set; } = [];
    public string PrimaryAdminName { get; set; } = string.Empty;
    public string PrimaryAdminEmail { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; } = true;
    public IReadOnlyList<BackofficeTenantAdminItem> AdminUsers { get; set; } = [];
    public IReadOnlyList<BackofficeTenantAuditLogItem> AuditLogs { get; set; } = [];
}

public sealed class BackofficeTenantProvisioningResult
{
    public BackofficeTenantWorkspaceItem Tenant { get; init; } = new();
    public string TemporaryPassword { get; init; } = string.Empty;
    public bool MustChangePassword { get; init; }
}

public sealed class BackofficeTenantAdminItem
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool MustChangePassword { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class BackofficeTenantAdminProvisioningResult
{
    public Guid TenantId { get; init; }
    public BackofficeTenantAdminItem Admin { get; init; } = new();
    public string TemporaryPassword { get; init; } = string.Empty;
    public bool MustChangePassword { get; init; }
}

public sealed class BackofficeTenantAuditLogItem
{
    public Guid Id { get; init; }
    public Guid? PlatformUserId { get; init; }
    public string PlatformUserName { get; init; } = string.Empty;
    public Guid? TenantId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string? MetadataJson { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class BackofficeTenantCreateCommand
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string PrimaryColor { get; init; } = "#2563eb";
    public string SecondaryColor { get; init; } = "#14b8a6";
    public bool IsActive { get; init; } = true;
    public IReadOnlyList<string> FeatureCodes { get; init; } = [];
    public string PrimaryAdminName { get; init; } = string.Empty;
    public string PrimaryAdminEmail { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
}

public sealed class BackofficeTenantUpdateCommand
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string PrimaryColor { get; init; } = "#2563eb";
    public string SecondaryColor { get; init; } = "#14b8a6";
    public bool IsActive { get; init; }
    public IReadOnlyList<string> FeatureCodes { get; init; } = [];
    public string AdminName { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
}

public sealed class BackofficeTenantAdminCreateCommand
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
