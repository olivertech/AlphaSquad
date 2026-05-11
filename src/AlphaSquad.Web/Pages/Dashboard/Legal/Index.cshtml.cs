using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Legal;

/// <summary>
/// Tela inicial de manutenção dos textos legais.
/// </summary>
public sealed class IndexModel : AdminDashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Gestão",
        Title = "Termos e políticas",
        Description = "Esta área reunirá os textos institucionais que serão lidos pelos usuários dentro do aplicativo da academia.",
        PanelTitle = "Documentos legais",
        PanelDescription = "Aqui ficarão os termos de uso e a política de privacidade que a academia publica para os usuários autenticados."
    };

    public IActionResult OnGet() => PageOrLogin();
}
