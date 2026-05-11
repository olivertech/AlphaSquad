using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Classes;

/// <summary>
/// Tela inicial do módulo de aulas e agendas.
/// </summary>
public sealed class IndexModel : DashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Operação",
        Title = "Aulas",
        Description = "Aqui ficarão as agendas, reservas e a gestão das turmas da academia em uma visualização clara para o dia a dia.",
        PanelTitle = "Agenda de aulas",
        PanelDescription = "Este espaço servirá para acompanhar aulas, reservas, capacidade e organização da rotina dos professores."
    };

    public IActionResult OnGet() => PageOrLogin();
}
