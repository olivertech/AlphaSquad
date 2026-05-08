namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um plano comercial disponibilizado pela academia para seus usuarios.
/// Esta entidade serve como catalogo de planos ativos dentro de cada tenant.
/// </summary>
public class MembershipPlan
{
    /// <summary>
    /// Identificador unico do plano.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant proprietario do plano.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nome comercial do plano.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descricao opcional do plano.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Preco atual do plano.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Quantidade de dias de vigencia padrao do plano.
    /// </summary>
    public int DurationDays { get; set; }

    /// <summary>
    /// Indica se o plano esta disponivel para novas atribuicoes.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Data de criacao do plano.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<UserMembership> UserMemberships { get; set; } = [];
}
