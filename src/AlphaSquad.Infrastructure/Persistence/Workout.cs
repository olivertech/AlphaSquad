namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um treino composto por vários exercícios.
/// </summary>
public class Workout
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lista de exercícios vinculados a este treino.
    /// </summary>
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
}
