using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class GamificationService : IGamificationService
{
    private readonly IApiFacade _apiFacade;

    public GamificationService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task<MyGamificationDashboardResponseDto?> GETApiGamificationMeAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiGamificationMeAsync(cancellationToken);
    }

    public Task<List<MonthlyRankingEntryResponseDto>?> GETApiGamificationRankingMonthlyAsync(int? month, int? top, int? year, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiGamificationRankingMonthlyAsync(month, top, year, cancellationToken);
    }

    public Task<List<GamificationEventRuleResponseDto>?> GETApiGamificationRulesAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiGamificationRulesAsync(cancellationToken);
    }

    public Task<List<MonthlyWinnerHistoryEntryResponseDto>?> GETApiGamificationWinnersHistoryAsync(int? limitMonths, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiGamificationWinnersHistoryAsync(limitMonths, cancellationToken);
    }

    public Task POSTApiGamificationRankingMonthlyCloseAsync(CloseMonthlyRankingRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiGamificationRankingMonthlyCloseAsync(request, cancellationToken);
    }

    public Task<GamificationEventRuleResponseDto?> POSTApiGamificationRulesAsync(CreateGamificationEventRuleRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiGamificationRulesAsync(request, cancellationToken);
    }

    public Task<GamificationEventRuleResponseDto?> PUTApiGamificationRulesByIdAsync(Guid id, UpdateGamificationEventRuleRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiGamificationRulesByIdAsync(id, request, cancellationToken);
    }
}