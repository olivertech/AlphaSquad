namespace AlphaSquad.Web.Dashboard.Metrics;

/// <summary>
/// Define um medidor disponível para o dashboard.
/// O catálogo é centralizado para reaproveitar rótulos, descrições e preferências em várias telas.
/// </summary>
public sealed class DashboardMetricDefinition
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required string ShortDescription { get; init; }
    public required string DetailDescription { get; init; }
    public required string SummaryLabel { get; init; }
    public required string SummaryValue { get; init; }
    public required string DetailPageTitle { get; init; }
    public required DashboardMetricChartKind ChartKind { get; init; }
    public required IReadOnlyList<string> Categories { get; init; }
    public required IReadOnlyList<double> Values { get; init; }
}
