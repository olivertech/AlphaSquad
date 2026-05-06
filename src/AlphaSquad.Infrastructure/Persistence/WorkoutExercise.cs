namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Entidade de associação entre Treinos e Exercícios, definindo a configuração do exercício no treino.
/// </summary>
public class WorkoutExercise
{
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int Order { get; set; }
    public int Sets { get; set; }
    public string Reps { get; set; } = string.Empty;
    public int? RestTime { get; set; }
    public string? Notes { get; set; }
}
