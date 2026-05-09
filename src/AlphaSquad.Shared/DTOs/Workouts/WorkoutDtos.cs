namespace AlphaSquad.Shared.DTOs.Workouts;

/// <summary>
/// DTO resumido usado nas listagens e retornos basicos do modulo de treinos.
/// </summary>
public record WorkoutResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Goal,
    bool IsActive,
    DateTime CreatedAt
);

/// <summary>
/// Payload de criacao de treino.
/// Reune apenas os campos principais definidos pela gestao da academia.
/// </summary>
public record WorkoutCreateRequest(
    string Name,
    string? Description,
    string? Goal
);

/// <summary>
/// Payload de atualizacao de treino.
/// Permite alterar os dados descritivos e o status ativo do registro.
/// </summary>
public record WorkoutUpdateRequest(
    string Name,
    string? Description,
    string? Goal,
    bool IsActive
);

/// <summary>
/// DTO de resposta para um exercicio dentro da composicao do treino.
/// Traz tanto a ordem quanto os parametros operacionais de execucao.
/// </summary>
public record WorkoutExerciseResponse(
    Guid ExerciseId,
    string ExerciseName,
    string? MediaUrl,
    int Order,
    int Sets,
    string Reps,
    int? RestTime,
    string? Notes
);

/// <summary>
/// DTO de detalhe completo do treino com a lista ordenada de exercicios associados.
/// </summary>
public record WorkoutDetailsResponse(
    WorkoutResponse Workout,
    List<WorkoutExerciseResponse> Exercises
);

/// <summary>
/// Payload usado para montar ou substituir a composicao de exercicios de um treino.
/// Cada item representa um exercicio com sua ordem e configuracao de execucao.
/// </summary>
public record ExerciseAssignmentRequest(
    Guid ExerciseId,
    int Order,
    int Sets,
    string Reps,
    int? RestTime,
    string? Notes
);
