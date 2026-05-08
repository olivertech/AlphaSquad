namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa a reserva de uma aula por um usuário.
/// Este registro liga um aluno a uma aula específica dentro do mesmo tenant.
/// </summary>
public class ClassBooking
{
    /// <summary>
    /// Identificador único global da reserva.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant ao qual a reserva pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Identificador da aula reservada.
    /// </summary>
    public Guid GymClassId { get; set; }

    /// <summary>
    /// Identificador do usuário que realizou a reserva.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Data e hora em que a reserva foi criada.
    /// </summary>
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Objeto de navegação para o tenant dono da reserva.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Objeto de navegação para a aula reservada.
    /// </summary>
    public GymClass GymClass { get; set; } = null!;

    /// <summary>
    /// Objeto de navegação para o usuário que realizou a reserva.
    /// </summary>
    public AppUser User { get; set; } = null!;
}
