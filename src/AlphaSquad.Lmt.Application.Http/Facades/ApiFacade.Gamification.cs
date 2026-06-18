using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Http.Mappers;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade
{
    public async Task<MyGamificationDashboardResponseDto?> GETApiGamificationMeAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Gamification.Me.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<MyGamificationDashboardResponseDto>(result);

    }

    public async Task<List<MonthlyRankingEntryResponseDto>?> GETApiGamificationRankingMonthlyAsync(int? month, int? top, int? year, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Gamification.Ranking.Monthly.GetAsync(config =>
        {
            config.QueryParameters.Month = month;
            config.QueryParameters.Top = top;
            config.QueryParameters.Year = year;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<MonthlyRankingEntryResponseDto>(result?.Items);

    }

    public async Task<List<GamificationEventRuleResponseDto>?> GETApiGamificationRulesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Gamification.Rules.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<GamificationEventRuleResponseDto>(result);

    }

    public async Task<List<MonthlyWinnerHistoryEntryResponseDto>?> GETApiGamificationWinnersHistoryAsync(int? limitMonths, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Gamification.Winners.History.GetAsync(config =>
        {
            config.QueryParameters.LimitMonths = limitMonths;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<MonthlyWinnerHistoryEntryResponseDto>(result);

    }

    public async Task POSTApiGamificationRankingMonthlyCloseAsync(CloseMonthlyRankingRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CloseMonthlyRankingRequest>(request);

        await _apiClient.Api.Gamification.Ranking.Monthly.Close.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

    }

    public async Task<GamificationEventRuleResponseDto?> POSTApiGamificationRulesAsync(CreateGamificationEventRuleRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateGamificationEventRuleRequest>(request);

        var result = await _apiClient.Api.Gamification.Rules.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<GamificationEventRuleResponseDto>(result);

    }

    public async Task<GamificationEventRuleResponseDto?> PUTApiGamificationRulesByIdAsync(Guid id, UpdateGamificationEventRuleRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateGamificationEventRuleRequest>(request);

        var result = await _apiClient.Api.Gamification.Rules[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<GamificationEventRuleResponseDto>(result);

    }
}
