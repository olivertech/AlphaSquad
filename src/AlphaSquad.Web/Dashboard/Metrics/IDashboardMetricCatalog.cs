namespace AlphaSquad.Web.Dashboard.Metrics;

/// <summary>
/// Expõe o catálogo completo de medidores que podem ser usados na home e nas telas analíticas.
/// </summary>
public interface IDashboardMetricCatalog
{
    IReadOnlyList<DashboardMetricDefinition> GetAll();
    DashboardMetricDefinition? GetByKey(string? key);
    IReadOnlyList<DashboardMetricDefinition> GetSelected(IReadOnlyCollection<string>? selectedKeys, int limit);
}
