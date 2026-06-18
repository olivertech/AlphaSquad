namespace AlphaSquad.Shared.DTOs.Events;

using AlphaSquad.Shared.DTOs.Common;

/// <summary>
/// DTO usado pela gestao para criar um novo evento no mural da academia.
/// </summary>
public record CreateAcademyEventRequest(
    string Title,
    string? Description,
    AcademyEventType EventType,
    Guid? MediaId,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsHighlighted,
    DateTime? HighlightStartsAt,
    DateTime? HighlightEndsAt,
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
    AcademyEventType EventType,
    Guid? MediaId,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsHighlighted,
    DateTime? HighlightStartsAt,
    DateTime? HighlightEndsAt,
    bool IsOutdoorEvent,
    bool AllowParticipation,
    bool IsActive
);

/// <summary>
/// Gera um destaque institucional de aniversariantes com texto automatico e janela temporaria de exibicao.
/// </summary>
public record GenerateBirthdayHighlightEventRequest(
    Guid? MediaId,
    DateTime? ReferenceDate,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt,
    DateTime? HighlightStartsAt,
    DateTime? HighlightEndsAt,
    bool IsActive
);

/// <summary>
/// Gera um destaque institucional com os vencedores ja fechados da gamificacao mensal.
/// </summary>
public record GenerateGamificationWinnersHighlightEventRequest(
    Guid? MediaId,
    int? Year,
    int? Month,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt,
    DateTime? HighlightStartsAt,
    DateTime? HighlightEndsAt,
    bool IsActive
);

/// <summary>
/// Resumo usado no dashboard para que a academia veja os aniversariantes do dia antes de publicar o destaque.
/// </summary>
public record BirthdayHighlightPreviewResponse(
    DateTime ReferenceDate,
    int BirthdayCount,
    List<string> StudentNames,
    bool CanGenerateHighlight
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
    AcademyEventType EventType,
    Guid? MediaId,
    string? MediaUrl,
    string? Location,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsHighlighted,
    DateTime? HighlightStartsAt,
    DateTime? HighlightEndsAt,
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
)
{
    public string EventTypeCode => MobileContractCodes.ToCode(EventType);

    public string StatusCode => IsCompleted
        ? "completed"
        : MobileContractCodes.FromBoolean(IsActive, "active", "inactive");

    public MobileCardItemResponse MobileCard => new(
        "event",
        Title,
        BuildSubtitle(),
        Description,
        MediaUrl,
        MediaUrl,
        EventMedia,
        BuildBadge(),
        StatusCode,
        HighlightStartsAt ?? StartsAt ?? CreatedAt,
        "events",
        Id,
        MobileContractCodes.BuildRouteHint("events", Id));

    public MediaPresentationResponse EventMedia => MediaPresentationFactory.ForEvent(Title, MediaUrl);

    private string? BuildSubtitle()
    {
        if (!string.IsNullOrWhiteSpace(Location))
            return Location;

        if (IsOutdoorEvent)
            return "evento presencial";

        return EventTypeCode;
    }

    private string? BuildBadge()
    {
        if (IsCompleted)
            return "concluido";

        if (IsHighlighted)
            return "destaque";

        if (AllowParticipation)
            return "participacao";

        return IsOutdoorEvent ? "presencial" : null;
    }
}

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
