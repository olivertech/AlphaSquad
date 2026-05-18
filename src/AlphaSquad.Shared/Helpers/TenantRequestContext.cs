namespace AlphaSquad.Shared.Helpers;

/// <summary>
/// Representa o contexto multi-tenant resolvido para a requisição atual.
/// Ele concentra as informações validadas do tenant e do usuário autenticado antes de chegar aos endpoints.
/// </summary>
public sealed class TenantRequestContext
{
    public Guid TenantId { get; init; }
    public string TenantSlug { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public bool IsTenantActive { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public UserRole UserRole { get; init; }
    public bool IsUserActive { get; init; }
}
