using AlphaSquad.Web.Dashboard.Metrics;

namespace AlphaSquad.Web.Pages.Dashboard.Metrics;

/// <summary>
/// Exibe a análise individual de um medidor do dashboard.
/// Cada bloco da home pode abrir esta tela para detalhar o indicador com um gráfico apropriado.
/// </summary>
public sealed class DetailsModel(IDashboardMetricCatalog metricCatalog) : DashboardPageModelBase
{
    public DashboardMetricDefinition? Metric { get; private set; }
    public object? ChartOptions { get; private set; }
    public string ChartExplanation { get; private set; } = string.Empty;

    public IActionResult OnGet(string metricKey)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        Metric = metricCatalog.GetByKey(metricKey);
        if (Metric is null)
            return Page();

        ChartExplanation = Metric.ChartKind switch
        {
            DashboardMetricChartKind.Line => "O gráfico de linha foi escolhido porque ele ajuda a enxergar evolução e comportamento ao longo do tempo.",
            DashboardMetricChartKind.Bar => "O gráfico de barras foi escolhido porque facilita comparações diretas entre categorias, turmas ou faixas de alunos.",
            DashboardMetricChartKind.Donut => "O gráfico em rosca foi escolhido porque ele ajuda a ver a divisão proporcional entre estados ou grupos do indicador.",
            _ => string.Empty
        };

        ChartOptions = BuildChartOptions(Metric);
        return Page();
    }

    private object BuildChartOptions(DashboardMetricDefinition metric)
    {
        var colors = new[]
        {
            SessionState?.PrimaryColor ?? "#14b8a6",
            SessionState?.SecondaryColor ?? "#475569",
            "#38bdf8",
            "#f59e0b",
            "#f43f5e"
        };

        return metric.ChartKind switch
        {
            DashboardMetricChartKind.Line => new
            {
                chart = new { type = "line", height = 360, toolbar = new { show = false } },
                stroke = new { curve = "smooth", width = 4 },
                colors = new[] { colors[0] },
                series = new[] { new { name = metric.Title, data = metric.Values } },
                xaxis = new { categories = metric.Categories },
                grid = new { borderColor = "#e2e8f0" }
            },
            DashboardMetricChartKind.Bar => new
            {
                chart = new { type = "bar", height = 360, toolbar = new { show = false } },
                plotOptions = new { bar = new { borderRadius = 8, distributed = true } },
                colors,
                series = new[] { new { name = metric.Title, data = metric.Values } },
                xaxis = new { categories = metric.Categories },
                legend = new { show = false },
                grid = new { borderColor = "#e2e8f0" }
            },
            DashboardMetricChartKind.Donut => new
            {
                chart = new { type = "donut", height = 360, toolbar = new { show = false } },
                labels = metric.Categories,
                colors,
                series = metric.Values,
                legend = new { position = "bottom" }
            },
            _ => new { }
        };
    }
}
