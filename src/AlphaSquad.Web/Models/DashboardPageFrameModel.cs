namespace AlphaSquad.Web.Models;

/// <summary>
/// Define a moldura visual padrão das páginas internas do dashboard.
/// Isso garante consistência entre módulos enquanto as telas reais ainda estão sendo construídas.
/// </summary>
public sealed class DashboardPageFrameModel
{
    public required string Section { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string PanelTitle { get; init; }
    public required string PanelDescription { get; init; }
}
