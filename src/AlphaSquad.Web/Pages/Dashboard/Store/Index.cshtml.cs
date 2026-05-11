using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Store;

/// <summary>
/// Tela inicial da loja da academia.
/// </summary>
public sealed class IndexModel : AdminDashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Operação",
        Title = "Loja",
        Description = "Este módulo concentrará catálogo, pedidos presenciais, retirada e pagamento local de produtos personalizados da academia.",
        PanelTitle = "Operação da loja",
        PanelDescription = "A área será usada para catálogo, variantes, pedidos e acompanhamento do fluxo presencial da loja."
    };

    public IActionResult OnGet() => PageOrLogin();
}
