namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa o registro de entrada de um usuário em uma determinada data.
/// Cada registro pertence a um tenant e a um usuário específico.
/// </summary>
public class CheckIn
{
    /// <summary>
    /// Identificador único global do registro de check-in.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant ao qual o check-in pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Identificador do usuário que realizou a entrada.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Data e hora exatas em que o check-in foi registrado.
    /// </summary>
    public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Observação opcional associada ao registro.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Objeto de navegação para o tenant dono do registro.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Objeto de navegação para o usuário que realizou o check-in.
    /// </summary>
    public AppUser User { get; set; } = null!;
}
