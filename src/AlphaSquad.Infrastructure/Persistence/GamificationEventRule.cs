namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Define quanto cada tipo de evento vale em pontos dentro de um tenant.
/// Esta entidade permite que a academia evolua suas regras de pontuacao sem alterar codigo.
/// </summary>
public class GamificationEventRule
{
    /// <summary>
    /// Identificador unico da regra.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant proprietario da regra.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Tipo do evento pontuavel.
    /// </summary>
    public GamificationEventType EventType { get; set; }

    /// <summary>
    /// Nome curto da regra para exibicao administrativa.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descricao opcional da regra.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Quantidade de pontos atribuida quando o evento ocorre.
    /// </summary>
    public decimal Points { get; set; }

    /// <summary>
    /// Indica se a regra pode ser usada para novas pontuacoes.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Data de criacao da regra.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}
