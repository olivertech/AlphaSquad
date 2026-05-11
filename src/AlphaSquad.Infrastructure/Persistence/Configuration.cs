namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa uma configuracao persistida por tenant e usuario.
/// A V1 usa esse registro para salvar as preferencias de medidores do dashboard.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Identificador unico do registro de configuracao.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual a configuracao pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Usuario dono da configuracao.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Chave logica do grupo de configuracao, por exemplo dashboard.metrics.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Valor serializado em JSON para permitir a evolucao de diferentes tipos de preferencia.
    /// </summary>
    public string ValueJson { get; set; } = string.Empty;

    /// <summary>
    /// Data de criacao inicial do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da ultima atualizacao da configuracao.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
