namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um plano de treino estruturado para um ou mais alunos.
/// Um treino é composto por uma sequência ordenada de exercícios com configurações específicas de séries e repetições.
/// </summary>
public class Workout
{
    /// <summary>
    /// Identificador único global do treino.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do Tenant (Academia) proprietária deste treino.
    /// Garante que treinos de uma academia não sejam visíveis ou editáveis por outra.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Objeto de navegação para a academia proprietária.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Nome do treino (ex: "Treino A - Hipertrofia", "Foco em Membros Inferiores").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do objetivo do treino ou instruções gerais para o aluno.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Objetivo específico do treino (ex: "Ganho de Massa", "Emagrecimento", "Condicionamento").
    /// </summary>
    public string? Goal { get; set; }

    /// <summary>
    /// Indica se o treino está ativo e disponível para ser atribuído a alunos.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lista de exercícios vinculados a este treino, incluindo a ordem de execução e volume (séries/reps).
    /// </summary>
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
}
