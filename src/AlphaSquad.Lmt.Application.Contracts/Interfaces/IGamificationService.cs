using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IGamificationService
{
    Task<MyGamificationDashboardResponseDto?> GETApiGamificationMeAsync(CancellationToken cancellationToken = default);

    Task<List<MonthlyRankingEntryResponseDto>?> GETApiGamificationRankingMonthlyAsync(int? month, int? top, int? year, CancellationToken cancellationToken = default);

    Task<List<GamificationEventRuleResponseDto>?> GETApiGamificationRulesAsync(CancellationToken cancellationToken = default);

    Task<List<MonthlyWinnerHistoryEntryResponseDto>?> GETApiGamificationWinnersHistoryAsync(int? limitMonths, CancellationToken cancellationToken = default);

    Task POSTApiGamificationRankingMonthlyCloseAsync(CloseMonthlyRankingRequestDto request, CancellationToken cancellationToken = default);

    Task<GamificationEventRuleResponseDto?> POSTApiGamificationRulesAsync(CreateGamificationEventRuleRequestDto request, CancellationToken cancellationToken = default);

    Task<GamificationEventRuleResponseDto?> PUTApiGamificationRulesByIdAsync(Guid id, UpdateGamificationEventRuleRequestDto request, CancellationToken cancellationToken = default);
}