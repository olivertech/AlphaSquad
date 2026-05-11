using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Gamification;

/// <summary>
/// Tela inicial da gamificação administrativa.
/// </summary>
public sealed class IndexModel : AdminDashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Comunidade",
        Title = "Gamificação",
        Description = "A área administrativa da gamificação concentrará ranking, pontuação e incentivos usados para manter os alunos engajados.",
        PanelTitle = "Central de pontuação",
        PanelDescription = "Aqui ficarão o ranking mensal, histórico de vencedores e regras usadas para premiar os alunos mais ativos."
    };

    public IActionResult OnGet() => PageOrLogin();
}
