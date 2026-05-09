namespace AlphaSquad.Shared.DTOs.Gamification;

/// <summary>
/// Resposta administrativa de regra de pontuacao.
/// </summary>
public record GamificationEventRuleResponse(
    Guid Id,
    GamificationEventType EventType,
    string Name,
    string? Description,
    decimal Points,
    bool IsActive,
    DateTime CreatedAt
);

/// <summary>
/// Payload para criacao de regra de pontuacao.
/// </summary>
public record CreateGamificationEventRuleRequest(
    GamificationEventType EventType,
    string Name,
    string? Description,
    decimal Points
);

/// <summary>
/// Payload para atualizacao de regra de pontuacao.
/// </summary>
public record UpdateGamificationEventRuleRequest(
    string Name,
    string? Description,
    decimal Points,
    bool IsActive
);

/// <summary>
/// Resposta de evento recente no dashboard do aluno.
/// </summary>
public record UserGamificationEventResponse(
    Guid Id,
    GamificationEventType EventType,
    string RuleName,
    decimal PointsApplied,
    string SourceEntity,
    Guid? SourceEntityId,
    DateTime OccurredAt,
    string? Notes
);

/// <summary>
/// Resposta resumida do ranking mensal.
/// </summary>
public record MonthlyRankingEntryResponse(
    Guid UserId,
    string UserName,
    decimal TotalPoints,
    int Position,
    string? PrizeDescription
);

/// <summary>
/// Resposta do dashboard de gamificacao do proprio aluno.
/// </summary>
public record MyGamificationDashboardResponse(
    Guid UserId,
    string UserName,
    decimal CurrentMonthPoints,
    int? CurrentMonthPosition,
    decimal TotalAccumulatedPoints,
    int CurrentMonthEventCount,
    List<UserGamificationEventResponse> RecentEvents
);

/// <summary>
/// Payload para fechar o ranking mensal e registrar vencedores.
/// </summary>
public record CloseMonthlyRankingRequest(
    int Year,
    int Month,
    string? FirstPlacePrize,
    string? SecondPlacePrize,
    string? ThirdPlacePrize
);
