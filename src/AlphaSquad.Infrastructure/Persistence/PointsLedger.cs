namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Mantem um razao auditavel de pontos por aluno.
/// Cada entrada corresponde a um evento processado e registra o saldo acumulado apos a operacao.
/// </summary>
public class PointsLedger
{
    /// <summary>
    /// Identificador unico do lancamento.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual o lancamento pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Usuario aluno que recebeu a variacao de pontos.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Evento de gamificacao que originou este lancamento.
    /// </summary>
    public Guid UserGamificationEventId { get; set; }

    /// <summary>
    /// Variacao de pontos aplicada neste lancamento.
    /// </summary>
    public decimal PointsDelta { get; set; }

    /// <summary>
    /// Saldo acumulado do aluno apos este lancamento.
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// Data de criacao do lancamento.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public UserGamificationEvent UserGamificationEvent { get; set; } = null!;
}
