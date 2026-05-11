using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Workouts;

/// <summary>
/// Tela inicial da montagem de treinos.
/// </summary>
public sealed class IndexModel : DashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Treinos",
        Title = "Treinos",
        Description = "Este módulo reunirá os programas de treino, suas combinações de exercícios e a organização do planejamento físico.",
        PanelTitle = "Programas de treino",
        PanelDescription = "A área será usada para montar, revisar e evoluir os treinos oferecidos pela academia."
    };

    public IActionResult OnGet() => PageOrLogin();
}
