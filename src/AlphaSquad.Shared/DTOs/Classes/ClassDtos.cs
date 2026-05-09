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
    int Capacity,
    bool IsSpecialClass
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
    bool IsActive,
    bool IsSpecialClass
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
    bool IsSpecialClass,
    DateTime CreatedAt
);

/// <summary>
/// DTO de resposta para uma reserva de aula.
/// </summary>
public record ClassBookingResponse(
    Guid Id,
    Guid GymClassId,
    string ClassName,
    Guid UserId,
    string UserName,
    DateTime BookedAt
);

/// <summary>
/// DTO usado em consultas operacionais de reservas para a equipe da academia.
/// Reune dados da aula, do aluno e do agendamento em uma unica projecao.
/// </summary>
public record ClassBookingManagementResponse(
    Guid BookingId,
    Guid GymClassId,
    string ClassName,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Location,
    bool IsSpecialClass,
    Guid UserId,
    string UserName,
    string UserEmail,
    UserRole UserRole,
    DateTime BookedAt
);
