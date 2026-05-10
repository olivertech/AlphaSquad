namespace AlphaSquad.Web.Security;

/// <summary>
/// Consolida o contexto autenticado do dashboard em um unico objeto de sessao.
/// Isso simplifica o consumo da API e a renderizacao da identidade atual nas paginas Razor.
/// </summary>
public sealed class DashboardSessionState
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public Guid? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public string? TenantName { get; set; }
    public string? TenantLogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
}
