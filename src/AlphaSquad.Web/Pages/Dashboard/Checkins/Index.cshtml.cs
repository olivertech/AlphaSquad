using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Checkins;
using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Checkins;

/// <summary>
/// Visão operacional do módulo de check-ins.
/// Ela ajuda a academia a acompanhar frequência recente e alunos que precisam de reengajamento.
/// </summary>
public sealed class IndexModel(ICheckinsService checkinsService) : AdminDashboardPageModelBase
{
    [BindProperty(SupportsGet = true)]
    public DateTime? DateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? DateTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public int DaysWithoutCheckIn { get; set; } = 7;

    public IReadOnlyList<CheckInListItemViewModel> CheckIns { get; private set; } = [];
    public IReadOnlyList<InactiveCheckInUserViewModel> InactiveUsers { get; private set; } = [];
    public int TotalCheckIns => CheckIns.Count;
    public int UniqueUsers => CheckIns.Select(item => item.UserId).Distinct().Count();
    public int TodayCheckIns => CheckIns.Count(item => item.CheckedInAt?.ToLocalTime().Date == DateTime.Today);
    public int ReengagementAlerts => InactiveUsers.Count;
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (DaysWithoutCheckIn <= 0)
            DaysWithoutCheckIn = 7;

        try
        {
            var checkInsResponse = await checkinsService.GETApiCheckinsTenantAsync(
                DateFrom,
                DateTo,
                1,
                100,
                null,
                cancellationToken);

            CheckIns = checkInsResponse?
                .Where(item => item.Id.HasValue && item.UserId.HasValue)
                .Select(CheckInPresentationMapper.ToListItem)
                .OrderByDescending(item => item.CheckedInAt ?? DateTimeOffset.MinValue)
                .ToList() ?? [];

            var inactiveUsersResponse = await checkinsService.GETApiCheckinsInactiveUsersAsync(DaysWithoutCheckIn, cancellationToken);
            InactiveUsers = inactiveUsersResponse?
                .Where(user => user.UserId.HasValue)
                .Select(CheckInPresentationMapper.ToInactiveUser)
                .OrderByDescending(user => user.DaysWithoutCheckInLabel)
                .ToList() ?? [];
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os dados de check-ins agora. Tente novamente em instantes.";
        }

        return result;
    }
}
