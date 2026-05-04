namespace AlphaSquad.Shared.Contracts;

public class TenantConfigResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
