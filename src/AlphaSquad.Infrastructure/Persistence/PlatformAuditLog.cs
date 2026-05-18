namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Registra a trilha administrativa das acoes do sponsor no backoffice master.
/// Essa auditoria ajuda a rastrear onboarding, alteracoes de branding e gestao de admins dos tenants.
/// </summary>
public class PlatformAuditLog
{
    public Guid Id { get; set; }
    public Guid? PlatformUserId { get; set; }
    public Guid? TenantId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PlatformUser? PlatformUser { get; set; }
    public Tenant? Tenant { get; set; }
}
