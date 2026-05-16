namespace AlphaSquad.Backoffice.Services;

/// <summary>
/// Modelo persistido em memoria para representar academias provisionadas pelo backoffice.
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
    public List<string> FeatureCodes { get; set; } = [];
    public string PrimaryAdminName { get; set; } = string.Empty;
    public string PrimaryAdminEmail { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; } = true;
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
    public string? LogoUrl { get; init; }
}
