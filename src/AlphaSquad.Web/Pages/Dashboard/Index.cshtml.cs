using AlphaSquad.Lmt.Application.Contracts.Dtos;
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
    IConfigurationsService configurationsService,
    IEventsService eventsService) : DashboardPageModelBase
{
    public IReadOnlyList<DashboardMetricDefinition> SelectedMetrics { get; private set; } = [];
    public BirthdayHighlightPreviewResponseDto? BirthdayPreview { get; private set; }
    public bool CanGenerateBirthdayHighlight => SessionState?.Role == DashboardRoles.Admin;

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

        await LoadBirthdayPreviewAsync(cancellationToken);

        return result;
    }

    public async Task<IActionResult> OnPostGenerateBirthdayHighlightAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (SessionState?.Role != DashboardRoles.Admin)
            return Forbid();

        try
        {
            var generated = await eventsService.POSTApiEventsInstitutionalBirthdaysGenerateAsync(
                new GenerateBirthdayHighlightEventRequestDto
                {
                    ReferenceDate = DateTimeOffset.UtcNow.Date,
                    IsActive = true
                },
                cancellationToken).ConfigureAwait(false);

            if (generated?.Id is null)
            {
                ShowWarningToast("Nenhum destaque foi gerado para os aniversariantes de hoje.", persist: true);
                return RedirectToPage();
            }

            ShowSuccessToast("Notificação de aniversariantes publicada com sucesso para os alunos.", persist: true);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            var message = ex.Message.Contains("No birthdays were found", StringComparison.OrdinalIgnoreCase)
                ? "Hoje não há aniversariantes para publicar no mural."
                : "Não foi possível gerar a publicação de aniversariantes agora.";

            ShowErrorToast(message, persist: true);
            return RedirectToPage();
        }
    }

    private async Task LoadBirthdayPreviewAsync(CancellationToken cancellationToken)
    {
        try
        {
            BirthdayPreview = await eventsService
                .GETApiEventsInstitutionalBirthdaysPreviewAsync(DateTimeOffset.UtcNow.Date, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            BirthdayPreview = null;
        }
    }
}
