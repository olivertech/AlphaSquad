namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa a confirmacao de participacao de um aluno em um evento do mural.
/// Esse registro serve tanto para experiencia do modulo quanto para alimentar a gamificacao.
/// </summary>
public class AcademyEventParticipation
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid AcademyEventId { get; set; }
    public AcademyEvent AcademyEvent { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public DateTime ParticipatedAt { get; set; }
    
    /// <summary>
    /// Indica se o aluno confirmou presença informando a senha do evento no app.
    /// </summary>
    public bool IsPresent { get; set; }
}
