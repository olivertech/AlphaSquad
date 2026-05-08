namespace AlphaSquad.Shared.DTOs.Classes;

/// <summary>
/// DTO utilizado para criar uma nova aula ou agenda.
/// </summary>
public record CreateGymClassRequest(
    string Name,
    string? Description,
    Guid? InstructorUserId,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Location,
    int Capacity
);

/// <summary>
/// DTO utilizado para atualizar uma aula existente.
/// </summary>
public record UpdateGymClassRequest(
    string Name,
    string? Description,
    Guid? InstructorUserId,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Location,
    int Capacity,
    bool IsActive
);

/// <summary>
/// DTO de resposta para listagem e detalhamento de aulas.
/// </summary>
public record GymClassResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? InstructorUserId,
    string? InstructorName,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Location,
    int Capacity,
    bool IsActive,
    DateTime CreatedAt
);
