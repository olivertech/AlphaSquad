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
/// Envelope do ranking mensal com informacoes explicitas do periodo consultado.
/// Isso ajuda o app e dashboards a saberem se o retorno vem de um snapshot fechado ou do ranking vivo.
/// </summary>
public record MonthlyRankingResponse(
    int Year,
    int Month,
    bool IsClosedSnapshot,
    List<MonthlyRankingEntryResponse> Items
);

/// <summary>
/// Item do historico de vencedores mensais da gamificacao.
/// </summary>
public record MonthlyWinnerHistoryEntryResponse(
    int Year,
    int Month,
    int Position,
    Guid UserId,
    string UserName,
    decimal TotalPoints,
    string? PrizeDescription,
    DateTime GeneratedAt
);

/// <summary>
/// Resposta do dashboard de gamificacao do proprio aluno.
/// </summary>
public record MyGamificationDashboardResponse(
    Guid UserId,
    string UserName,
    int CurrentYear,
    int CurrentMonth,
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

/// <summary>
/// Resposta amigavel usada quando o usuario autenticado nao participa da gamificacao.
/// Esse contrato evita respostas vazias e deixa a regra de negocio clara para o consumidor da API.
/// </summary>
public record GamificationAccessMessageResponse(
    string Message
);
