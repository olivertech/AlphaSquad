using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Events;

/// <summary>
/// Tela inicial do mural de eventos da academia.
/// </summary>
public sealed class IndexModel : DashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Operação",
        Title = "Eventos",
        Description = "O mural de eventos reunirá comunicados, ações externas e experiências promovidas pela academia para engajar a comunidade.",
        PanelTitle = "Mural de eventos",
        PanelDescription = "Esta área receberá as publicações institucionais, participações confirmadas e futuras ações promovidas pela academia."
    };

    public IActionResult OnGet() => PageOrLogin();
}
