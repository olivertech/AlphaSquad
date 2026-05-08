namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um pedido realizado no app para retirada presencial e pagamento local na academia.
/// O pedido registra o ciclo operacional da loja antes da futura integracao com gateway externo.
/// </summary>
public class StoreOrder
{
    /// <summary>
    /// Identificador unico do pedido.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual o pedido pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Usuario que realizou o pedido.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Status operacional atual do pedido.
    /// </summary>
    public StoreOrderStatus Status { get; set; } = StoreOrderStatus.PendingApproval;

    /// <summary>
    /// Valor total do pedido no momento da criacao.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Observacoes livres do aluno no momento da compra.
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Observacoes administrativas sobre separacao, retirada ou pagamento.
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Usuario que realizou a ultima mudanca operacional relevante no pedido.
    /// </summary>
    public Guid? LastUpdatedByUserId { get; set; }

    /// <summary>
    /// Data em que o pedido foi criado.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da ultima atualizacao do pedido.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public AppUser? LastUpdatedByUser { get; set; }
    public ICollection<StoreOrderItem> Items { get; set; } = [];
}
