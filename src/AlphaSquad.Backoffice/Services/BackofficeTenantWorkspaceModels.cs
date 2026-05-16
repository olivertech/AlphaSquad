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
}

public sealed class BackofficeTenantProvisioningResult
{
    public BackofficeTenantWorkspaceItem Tenant { get; init; } = new();
    public string TemporaryPassword { get; init; } = string.Empty;
    public bool MustChangePassword { get; init; }
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
