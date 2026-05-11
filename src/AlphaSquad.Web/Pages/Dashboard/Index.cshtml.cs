using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Dashboard.Metrics;
using AlphaSquad.Web.Security;

namespace AlphaSquad.Web.Pages.Dashboard;

/// <summary>
/// Home inicial do dashboard.
/// Nesta primeira fase, ela resume os principais medidores escolhidos pelo usuário para a academia.
/// </summary>
public sealed class IndexModel(
    IDashboardMetricCatalog metricCatalog,
    IConfigurationsService configurationsService) : DashboardPageModelBase
{
    public IReadOnlyList<DashboardMetricDefinition> SelectedMetrics { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        // A sessao funciona como cache local do dashboard.
        // Quando ainda nao ha selecao carregada, consultamos a configuracao persistida do usuario.
        if (SessionState is not null && SessionState.SelectedDashboardMetricKeys.Count == 0)
        {
            try
            {
                var persistedConfiguration = await configurationsService
                    .GETApiConfigurationsMeDashboardAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (persistedConfiguration?.SelectedMetricKeys?.Count > 0)
                {
                    SessionState.SelectedDashboardMetricKeys = [.. persistedConfiguration.SelectedMetricKeys];
                    HttpContext.Session.SetDashboardSession(SessionState);
                }
            }
            catch
            {
                // Se a leitura falhar, a home continua com o fallback visual padrao para nao bloquear o acesso ao painel.
            }
        }

        SelectedMetrics = metricCatalog.GetSelected(
            SessionState?.SelectedDashboardMetricKeys,
            DashboardMetricCatalog.MaxDashboardMetrics);

        return result;
    }
}
