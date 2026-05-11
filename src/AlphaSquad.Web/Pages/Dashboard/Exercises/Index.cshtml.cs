using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Exercises;

/// <summary>
/// Tela inicial do catálogo de exercícios.
/// </summary>
public sealed class IndexModel : DashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Treinos",
        Title = "Exercícios",
        Description = "Este espaço reunirá o catálogo de exercícios da academia, com mídia, instruções e estrutura para os treinos.",
        PanelTitle = "Biblioteca de exercícios",
        PanelDescription = "A página será usada para cadastrar, revisar e organizar os movimentos que alimentam os planos de treino."
    };

    public IActionResult OnGet() => PageOrLogin();
}
