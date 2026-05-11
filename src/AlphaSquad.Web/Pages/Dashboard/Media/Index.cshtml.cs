using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Media;

/// <summary>
/// Tela inicial da biblioteca de mídias.
/// </summary>
public sealed class IndexModel : AdminDashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Gestão",
        Title = "Mídias",
        Description = "Aqui ficarão uploads, organização visual e controle dos arquivos usados nos demais módulos da academia.",
        PanelTitle = "Biblioteca de arquivos",
        PanelDescription = "Esta página será usada para manter fotos, imagens e recursos visuais que abastecem o ecossistema do AlphaSquad."
    };

    public IActionResult OnGet() => PageOrLogin();
}
