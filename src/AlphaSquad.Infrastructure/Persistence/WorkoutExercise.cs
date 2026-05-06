namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Entidade de associação (Join Table) entre Treinos e Exercícios.
/// Esta classe não apenas vincula as duas entidades, mas armazena a "configuração" do exercício 
/// dentro de um treino específico (ex: o mesmo exercício pode ter séries diferentes em treinos distintos).
/// </summary>
public class WorkoutExercise
{
    /// <summary>
    /// Chave estrangeira para o Treino ao qual este exercício pertence.
    /// </summary>
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;

    /// <summary>
    /// Chave estrangeira para o Exercício a ser executado.
    /// </summary>
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    /// <summary>
    /// Posição do exercício na sequência do treino (ex: 1º, 2º, 3º).
    /// Essencial para manter a ordem lógica da sessão de treino.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Quantidade de séries recomendadas para este exercício neste treino.
    /// </summary>
    public int Sets { get; set; }

    /// <summary>
    /// Repetições recomendadas. Armazenado como string para permitir intervalos (ex: "12-15") 
    /// ou termos como "Até a falha".
    /// </summary>
    public string Reps { get; set; } = string.Empty;

    /// <summary>
    /// Tempo de descanso sugerido entre as séries, em segundos.
    /// </summary>
    public int? RestTime { get; set; }

    /// <summary>
    /// Observações específicas sobre a execução deste exercício dentro deste contexto de treino.
    /// </summary>
    public string? Notes { get; set; }
}
