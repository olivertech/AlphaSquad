namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um exercício físico disponível para a academia.
/// Esta entidade é a unidade básica de atividade física no sistema, 
/// podendo ser compartilhada por múltiplos treinos de diferentes alunos.
/// </summary>
public class Exercise
{
    /// <summary>
    /// Identificador único global do exercício.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do Tenant (Academia) proprietária deste exercício.
    /// Garante o isolamento de dados entre diferentes academias no modelo multi-tenant.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nome descritivo do exercício (ex: "Supino Reto", "Agachamento Livre").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detalhes adicionais sobre a execução do exercício ou instruções técnicas.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Grupo muscular principal trabalhado (ex: "Peitoral", "Quadríceps").
    /// Útil para filtragem e organização de treinos.
    /// </summary>
    public string MuscleGroup { get; set; } = string.Empty;
    
    /// <summary>
    /// Referência para a mídia (vídeo/imagem) que demonstra a execução correta do exercício.
    /// Vinculada à entidade TenantMedia para gerenciamento de arquivos no storage.
    /// </summary>
    public Guid? MediaId { get; set; }
    public TenantMedia? Media { get; set; }
    
    /// <summary>
    /// Data e hora de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Conjunto de associações deste exercício com diversos treinos.
    /// </summary>
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
}
