using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Academy;

/// <summary>
/// Porta de entrada da configuração institucional da academia.
/// </summary>
public sealed class IndexModel : AdminDashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Gestão",
        Title = "Academia",
        Description = "Aqui ficarão as configurações institucionais, branding e ajustes centrais da academia dentro do painel.",
        PanelTitle = "Configurações da academia",
        PanelDescription = "Este espaço será usado para identidade visual, dados principais e parâmetros administrativos do tenant."
    };

    public IActionResult OnGet() => PageOrLogin();
}
