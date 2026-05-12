using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Plans;

namespace AlphaSquad.Web.Pages.Dashboard.Plans;

/// <summary>
/// Visão operacional do módulo de planos.
/// Ela destaca o catálogo atual e ajuda a gestão a enxergar sinais de retenção com foco em alunos sem plano ativo.
/// </summary>
public sealed class IndexModel(IPlansService plansService) : AdminDashboardPageModelBase
{
    [BindProperty(SupportsGet = true)]
    public int DaysWithoutPlan { get; set; } = 30;

    public IReadOnlyList<PlanListItemViewModel> Plans { get; private set; } = [];
    public IReadOnlyList<InactivePlanUserViewModel> InactiveUsers { get; private set; } = [];
    public int TotalPlans => Plans.Count;
    public int ActivePlans => Plans.Count(plan => plan.IsActive);
    public double AveragePrice => Plans.Count == 0 ? 0d : Plans.Average(plan => ParseCurrency(plan.PriceLabel));
    public int InactiveStudents => InactiveUsers.Count;
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (DaysWithoutPlan <= 0)
            DaysWithoutPlan = 30;

        try
        {
            var plansResponse = await plansService.GETApiPlansAsync(cancellationToken);
            Plans = plansResponse?
                .Where(plan => plan.Id.HasValue)
                .Select(PlanPresentationMapper.ToListItem)
                .OrderBy(plan => plan.Name)
                .ToList() ?? [];

            var inactiveUsersResponse = await plansService.GETApiPlansInactiveUsersAsync(DaysWithoutPlan, cancellationToken);
            InactiveUsers = inactiveUsersResponse?
                .Where(user => user.UserId.HasValue)
                .Select(PlanPresentationMapper.ToInactiveUser)
                .OrderByDescending(user => user.LastPlanEndedAt ?? DateTimeOffset.MinValue)
                .ToList() ?? [];
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os dados de planos agora. Tente novamente em instantes.";
            ShowErrorToast(LoadErrorMessage);
        }

        return result;
    }

    /// <summary>
    /// Reaproveita o valor monetário já apresentado no card para gerar uma média simples na própria tela.
    /// </summary>
    private static double ParseCurrency(string currencyLabel)
    {
        var digitsOnly = new string(currencyLabel.Where(ch => char.IsDigit(ch) || ch == ',' || ch == '.').ToArray());

        return double.TryParse(digitsOnly.Replace(".", string.Empty).Replace(',', '.'),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var value)
            ? value
            : 0d;
    }
}
