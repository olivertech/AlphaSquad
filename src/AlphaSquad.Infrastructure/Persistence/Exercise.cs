namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um exercício físico disponível para a academia.
/// </summary>
public class Exercise
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MuscleGroup { get; set; } = string.Empty;
    
    /// <summary>
    /// Referência para a mídia (vídeo/imagem) do exercício.
    /// </summary>
    public Guid? MediaId { get; set; }
    public TenantMedia? Media { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
}
