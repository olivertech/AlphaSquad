using AlphaSquad.Shared.DTOs.Auth;
using AlphaSquad.Shared.DTOs.Tenants;

namespace AlphaSquad.Shared.DTOs.App;

/// <summary>
/// Payload inicial do app mobile para montar a shell autenticada sem precisar disparar varias chamadas em paralelo.
/// Consolida sessao, branding, modulos liberados e sinais rapidos da home do aluno.
/// </summary>
public record AppBootstrapResponse(
    DateTime GeneratedAtUtc,
    AuthenticatedSessionResponse Session,
    TenantCurrentResponse Tenant,
    TenantFeaturesResponse Features,
    int UnreadNotificationsCount,
    AppBootstrapGamificationSummaryResponse? Gamification
)
{
    public bool HasUnreadNotifications => UnreadNotificationsCount > 0;
}

/// <summary>
/// Resumo leve de gamificacao usado no bootstrap do app.
/// O objetivo e evitar que a tela inicial precise carregar o dashboard completo logo no primeiro paint.
/// </summary>
public record AppBootstrapGamificationSummaryResponse(
    int CurrentYear,
    int CurrentMonth,
    decimal CurrentMonthPoints,
    int? CurrentMonthPosition,
    decimal TotalAccumulatedPoints,
    int CurrentMonthEventCount
);
