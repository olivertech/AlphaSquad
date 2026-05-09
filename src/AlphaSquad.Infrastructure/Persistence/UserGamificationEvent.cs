namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Registra cada evento de gamificacao realmente processado para um aluno.
/// Serve como base do ranking mensal, historico pessoal e prevencao de duplicidade.
/// </summary>
public class UserGamificationEvent
{
    /// <summary>
    /// Identificador unico do evento registrado.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual o evento pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Usuario aluno que recebeu os pontos.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Regra usada para atribuir os pontos.
    /// </summary>
    public Guid GamificationEventRuleId { get; set; }

    /// <summary>
    /// Tipo do evento realizado.
    /// </summary>
    public GamificationEventType EventType { get; set; }

    /// <summary>
    /// Nome tecnico da origem do evento, como checkin, store_order ou plan_renewal.
    /// </summary>
    public string SourceEntity { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do recurso de origem, quando existir.
    /// </summary>
    public Guid? SourceEntityId { get; set; }

    /// <summary>
    /// Quantidade de pontos efetivamente aplicada neste evento.
    /// </summary>
    public decimal PointsApplied { get; set; }

    /// <summary>
    /// Data em que o fato de negocio ocorreu.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Observacao opcional para auditoria ou leitura administrativa.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Data de persistencia do evento de gamificacao.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public GamificationEventRule GamificationEventRule { get; set; } = null!;
}
