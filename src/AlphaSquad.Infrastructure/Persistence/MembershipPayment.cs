namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Registra um pagamento de mensalidade ou plano associado a um aluno.
/// Esta entidade permite auditoria comercial e serve de base para pontuar pagamentos em dia.
/// </summary>
public class MembershipPayment
{
    /// <summary>
    /// Identificador unico do pagamento registrado.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual o pagamento pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Usuario aluno ao qual o pagamento se refere.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Plano ao qual o pagamento esta associado.
    /// </summary>
    public Guid MembershipPlanId { get; set; }

    /// <summary>
    /// Vinculo de plano ativo ou historico relacionado a este pagamento, quando existir.
    /// </summary>
    public Guid? UserMembershipId { get; set; }

    /// <summary>
    /// Valor pago pelo aluno.
    /// </summary>
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// Data limite prevista para pagamento.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Data efetiva em que o pagamento foi recebido.
    /// </summary>
    public DateTime PaidAt { get; set; }

    /// <summary>
    /// Indica se o pagamento foi recebido ate a data limite.
    /// </summary>
    public bool IsPaidOnTime { get; set; }

    /// <summary>
    /// Usuario que registrou administrativamente o pagamento.
    /// </summary>
    public Guid RecordedByUserId { get; set; }

    /// <summary>
    /// Observacoes administrativas opcionais.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Data de criacao do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public UserMembership? UserMembership { get; set; }
    public AppUser RecordedByUser { get; set; } = null!;
}
