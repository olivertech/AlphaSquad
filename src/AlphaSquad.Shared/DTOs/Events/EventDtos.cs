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
    DateTime ParticipatedAt
);
