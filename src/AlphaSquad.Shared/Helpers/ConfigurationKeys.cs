namespace AlphaSquad.Shared.Helpers;

/// <summary>
/// Centraliza as chaves usadas para armazenar configuracoes persistidas por tenant e usuario.
/// Isso evita strings soltas e facilita a evolucao de novos grupos de preferencia no futuro.
/// </summary>
public static class ConfigurationKeys
{
    /// <summary>
    /// Identifica a configuracao dos medidores exibidos na home do dashboard.
    /// </summary>
    public const string DashboardMetrics = "dashboard.metrics";
}
