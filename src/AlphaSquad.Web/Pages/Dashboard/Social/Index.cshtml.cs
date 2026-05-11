using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Social;

/// <summary>
/// Tela inicial da rede social interna da academia.
/// </summary>
public sealed class IndexModel : DashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Comunidade",
        Title = "Social",
        Description = "Aqui ficará a visão administrativa da rede social da academia, fortalecendo a integração entre alunos, professores e gestão.",
        PanelTitle = "Comunidade da academia",
        PanelDescription = "Esta área será usada para acompanhar publicações, comentários e sinais de engajamento da comunidade."
    };

    public IActionResult OnGet() => PageOrLogin();
}
