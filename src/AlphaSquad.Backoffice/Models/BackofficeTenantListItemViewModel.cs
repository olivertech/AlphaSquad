namespace AlphaSquad.Backoffice.Models;

/// <summary>
/// Resume uma academia na grade consolidada do backoffice.
/// </summary>
public sealed class BackofficeTenantListItemViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public string PrimaryColor { get; init; } = "#2563eb";
    public string SecondaryColor { get; init; } = "#14b8a6";
    public string? LogoUrl { get; init; }
    public string PrimaryAdminName { get; init; } = string.Empty;
    public string PrimaryAdminEmail { get; init; } = string.Empty;
    public int FeatureCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
