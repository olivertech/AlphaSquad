namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa as preferências visuais do dashboard mantidas na sessão da V1.
/// A estrutura já nasce pronta para ser persistida no backend futuramente.
/// </summary>
public sealed class DashboardSettingsInputModel
{
    public List<string> SelectedMetricKeys { get; set; } = [];
}
