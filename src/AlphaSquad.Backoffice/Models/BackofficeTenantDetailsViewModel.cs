namespace AlphaSquad.Backoffice.Models;

/// <summary>
/// Leva para a pagina de detalhes uma visao completa da academia provisionada pela AlphaSquad.
/// </summary>
public sealed class BackofficeTenantDetailsViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string PrimaryColor { get; init; } = "#2563eb";
    public string SecondaryColor { get; init; } = "#14b8a6";
    public IReadOnlyList<BackofficeFeatureOptionViewModel> Features { get; init; } = [];
    public string PrimaryAdminName { get; init; } = string.Empty;
    public string PrimaryAdminEmail { get; init; } = string.Empty;
    public string TemporaryPassword { get; init; } = string.Empty;
    public bool MustChangePassword { get; init; }
    public DateTime CreatedAt { get; init; }
}
