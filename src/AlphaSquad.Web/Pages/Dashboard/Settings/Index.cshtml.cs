using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Dashboard.Metrics;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Security;

namespace AlphaSquad.Web.Pages.Dashboard.Settings;

/// <summary>
/// Centraliza as preferências visuais do painel administrativo.
/// Nesta fase, cada usuário já pode salvar em banco quais medidores deseja ver na home.
/// </summary>
public sealed class IndexModel(
    IDashboardMetricCatalog metricCatalog,
    IConfigurationsService configurationsService) : DashboardPageModelBase
{
    [BindProperty]
    public DashboardSettingsInputModel Input { get; set; } = new();

    public IReadOnlyList<DashboardMetricDefinition> AvailableMetrics { get; private set; } = [];
    public int MaxSelectableMetrics => DashboardMetricCatalog.MaxDashboardMetrics;
    public string? SelectionErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        await LoadViewStateAsync(cancellationToken);
        return result;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        AvailableMetrics = metricCatalog.GetAll();
        Input.SelectedMetricKeys = Input.SelectedMetricKeys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (Input.SelectedMetricKeys.Count == 0)
        {
            SelectionErrorMessage = "Selecione pelo menos um medidor para aparecer na home do dashboard.";
            ShowWarningToast(SelectionErrorMessage);
            return Page();
        }

        if (Input.SelectedMetricKeys.Count > DashboardMetricCatalog.MaxDashboardMetrics)
        {
            SelectionErrorMessage = $"Selecione no máximo {DashboardMetricCatalog.MaxDashboardMetrics} medidores para a tela inicial.";
            ShowWarningToast(SelectionErrorMessage);
            return Page();
        }

        SessionState ??= HttpContext.Session.GetDashboardSession();
        if (SessionState is null)
            return RedirectToPage("/Account/Login");

        try
        {
            var response = await configurationsService
                .PUTApiConfigurationsMeDashboardAsync(new UpdateDashboardConfigurationRequestDto
                {
                    SelectedMetricKeys = [.. Input.SelectedMetricKeys]
                }, cancellationToken)
                .ConfigureAwait(false);

            SessionState.SelectedDashboardMetricKeys = response?.SelectedMetricKeys?.Count > 0
                ? [.. response.SelectedMetricKeys]
                : [.. Input.SelectedMetricKeys];

            HttpContext.Session.SetDashboardSession(SessionState);
            SuccessMessage = "Os medidores da tela inicial foram salvos com sucesso para o seu usuário.";
            ShowSuccessToast(SuccessMessage);
        }
        catch
        {
            SelectionErrorMessage = "Não foi possível salvar suas preferências agora. Tente novamente em instantes.";
            ShowErrorToast(SelectionErrorMessage);
        }

        return Page();
    }

    /// <summary>
    /// Carrega a seleção atual de medidores do banco e replica a escolha na sessão para manter a home sincronizada.
    /// </summary>
    private async Task LoadViewStateAsync(CancellationToken cancellationToken)
    {
        AvailableMetrics = metricCatalog.GetAll();

        var selectedMetricKeys = metricCatalog.GetSelected([], DashboardMetricCatalog.MaxDashboardMetrics)
            .Select(metric => metric.Key)
            .ToList();

        try
        {
            var response = await configurationsService
                .GETApiConfigurationsMeDashboardAsync(cancellationToken)
                .ConfigureAwait(false);

            if (response?.SelectedMetricKeys?.Count > 0)
                selectedMetricKeys = [.. response.SelectedMetricKeys];
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar as preferências salvas agora. O painel exibiu uma seleção padrão temporária.";
            ShowWarningToast(LoadErrorMessage);
        }

        SessionState ??= HttpContext.Session.GetDashboardSession();
        if (SessionState is not null)
        {
            SessionState.SelectedDashboardMetricKeys = selectedMetricKeys;
            HttpContext.Session.SetDashboardSession(SessionState);
        }

        Input = new DashboardSettingsInputModel
        {
            SelectedMetricKeys = selectedMetricKeys
        };
    }
}
