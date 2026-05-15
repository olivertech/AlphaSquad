namespace AlphaSquad.Shared.DTOs.Events;

/// <summary>
/// DTO usado pela gestao para criar um novo evento no mural da academia.
/// </summary>
public record CreateAcademyEventRequest(
    string Title,
    string? Description,
    Guid? MediaId,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsOutdoorEvent,
    bool AllowParticipation,
    bool IsActive
);

/// <summary>
/// DTO usado pela gestao para atualizar um evento existente.
/// </summary>
public record UpdateAcademyEventRequest(
    string Title,
    string? Description,
    Guid? MediaId,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsOutdoorEvent,
    bool AllowParticipation,
    bool IsActive
);

/// <summary>
/// DTO usado pelo app para confirmar presenca no evento informando a senha divulgada pela academia.
/// </summary>
public record EventCheckInRequest(
    string Password
);

/// <summary>
/// DTO de resposta para o encerramento administrativo de um evento.
/// Ele informa quantos alunos presentes receberam pontos quando o gestor concluiu o evento.
/// </summary>
public record CompleteAcademyEventResponse(
    Guid EventId,
    string EventTitle,
    int AwardedParticipantsCount,
    bool IsCompleted,
    bool IsActive
);

/// <summary>
/// DTO de resposta para listagem e detalhe do mural de eventos.
/// </summary>
public record AcademyEventResponse(
    Guid Id,
    string Title,
    string? Description,
    Guid? MediaId,
    string? MediaUrl,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsOutdoorEvent,
    bool AllowParticipation,
    bool IsActive,
    string? CheckInPassword,
    bool IsCompleted,
    Guid CreatedByUserId,
    string CreatedByUserName,
    int ParticipantCount,
    bool IsUserParticipating,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// DTO de resposta para confirmar participacao de um aluno em evento outdoor.
/// </summary>
public record AcademyEventParticipationResponse(
    Guid Id,
    Guid AcademyEventId,
    string EventTitle,
    Guid UserId,
    string UserName,
    string? UserEmail,
    bool IsPresent,
    DateTime ParticipatedAt
);
