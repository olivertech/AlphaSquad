namespace AlphaSquad.Shared.Contracts;

public class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
}