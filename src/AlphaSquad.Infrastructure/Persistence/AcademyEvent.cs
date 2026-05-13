namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa uma publicacao institucional ou outdoor da academia dentro do mural do tenant.
/// O mesmo dominio atende eventos presenciais, acoes sociais e comunicados relevantes da gestao.
/// </summary>
public class AcademyEvent
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? MediaId { get; set; }
    public TenantMedia? Media { get; set; }

    public string? Location { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    /// <summary>
    /// Indica se o registro representa um evento outdoor que pode participar da gamificacao.
    /// </summary>
    public bool IsOutdoorEvent { get; set; }

    /// <summary>
    /// Controla se alunos podem confirmar participacao no app.
    /// </summary>
    public bool AllowParticipation { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Senha aleatória gerada na criação do evento, usada para check-in.
    /// </summary>
    public string CheckInPassword { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o evento foi confirmado/concluído pelo administrador.
    /// </summary>
    public bool IsCompleted { get; set; }
    public Guid CreatedByUserId { get; set; }
    public AppUser CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AcademyEventParticipation> Participations { get; set; } = [];
}
