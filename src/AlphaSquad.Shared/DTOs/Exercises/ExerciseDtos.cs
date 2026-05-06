namespace AlphaSquad.Shared.DTOs.Exercises;

/// <summary>
/// DTO utilizado para a criação de um novo exercício.
/// </summary>
public record ExerciseCreateRequest(
    string Name,
    string MuscleGroup,
    string? Description = null,
    Guid? MediaId = null
);

/// <summary>
/// DTO utilizado para a atualização de um exercício existente.
/// </summary>
public record ExerciseUpdateRequest(
    string Name,
    string MuscleGroup,
    string? Description = null,
    Guid? MediaId = null
);

/// <summary>
/// DTO de resposta para a listagem ou detalhamento de um exercício.
/// </summary>
public record ExerciseResponse(
    Guid Id,
    string Name,
    string MuscleGroup,
    string? Description,
    Guid? MediaId,
    string? MediaUrl,
    DateTime CreatedAt
);
