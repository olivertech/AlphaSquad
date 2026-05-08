namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa o vinculo de um usuario com um plano da academia.
/// Permite historico e identificacao do plano ativo em cada momento.
/// </summary>
public class UserMembership
{
    /// <summary>
    /// Identificador unico do vinculo.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual o vinculo pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Usuario associado ao plano.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Plano associado ao usuario.
    /// </summary>
    public Guid MembershipPlanId { get; set; }

    /// <summary>
    /// Data de inicio da vigencia.
    /// </summary>
    public DateTime StartsAt { get; set; }

    /// <summary>
    /// Data de termino da vigencia.
    /// </summary>
    public DateTime? EndsAt { get; set; }

    /// <summary>
    /// Indica se este vinculo e o plano ativo atual do usuario.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Motivo do status atual do vinculo.
    /// Pode registrar, por exemplo, upgrade, cancelamento, pausa ou campanha comercial.
    /// </summary>
    public string? StatusReason { get; set; }

    /// <summary>
    /// Usuario que realizou a ultima mudanca relevante neste vinculo.
    /// Ajuda em auditoria administrativa e leitura de historico.
    /// </summary>
    public Guid? ChangedByUserId { get; set; }

    /// <summary>
    /// Data de criacao do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public AppUser? ChangedByUser { get; set; }
}
