namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Armazena os textos legais exibidos no app para um tenant.
/// Esse registro centraliza Termos de Uso e Politica de Privacidade com manutencao administrativa.
/// </summary>
public class TenantLegalContent
{
    /// <summary>
    /// Identificador unico do registro legal.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual os textos legais pertencem.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Texto completo dos Termos de Uso apresentados no app.
    /// </summary>
    public string TermsOfUse { get; set; } = string.Empty;

    /// <summary>
    /// Texto completo da Politica de Privacidade apresentada no app.
    /// </summary>
    public string PrivacyPolicy { get; set; } = string.Empty;

    /// <summary>
    /// Usuario administrador que realizou a ultima alteracao dos textos.
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }

    /// <summary>
    /// Data de criacao inicial do registro legal.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da ultima atualizacao dos textos legais.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser? UpdatedByUser { get; set; }
}
