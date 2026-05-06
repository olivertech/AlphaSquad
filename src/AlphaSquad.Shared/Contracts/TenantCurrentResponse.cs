namespace AlphaSquad.Shared.Contracts;

/// <summary>
/// Resposta contendo as informações do Tenant (academia) atualmente autenticado.
/// </summary>
public class TenantCurrentResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
