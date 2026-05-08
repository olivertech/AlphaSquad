namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa uma aula ou agenda disponível dentro da academia.
/// Este registro define quando a aula acontece, quem pode ministrá-la e qual a sua capacidade.
/// </summary>
public class GymClass
{
    /// <summary>
    /// Identificador único global da aula.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant ao qual esta aula pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nome principal da aula (ex: "Spinning", "Yoga", "Funcional 18h").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrição opcional com detalhes da dinâmica da aula.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Usuário responsável por ministrar a aula, quando existir.
    /// </summary>
    public Guid? InstructorUserId { get; set; }

    /// <summary>
    /// Data e hora de início da aula.
    /// </summary>
    public DateTime StartsAt { get; set; }

    /// <summary>
    /// Data e hora de término da aula.
    /// </summary>
    public DateTime EndsAt { get; set; }

    /// <summary>
    /// Local opcional onde a aula acontecerá.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Quantidade máxima de participantes permitidos.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Indica se a aula está ativa e disponível para consulta.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Objeto de navegação para o tenant dono da aula.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Objeto de navegação para o instrutor vinculado à aula.
    /// </summary>
    public AppUser? InstructorUser { get; set; }
}
