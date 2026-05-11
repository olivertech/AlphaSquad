using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Profile;

/// <summary>
/// Tela inicial do perfil administrativo.
/// </summary>
public sealed class IndexModel : DashboardPageModelBase
{
    public DashboardPageFrameModel PageFrame { get; } = new()
    {
        Section = "Conta",
        Title = "Meu perfil",
        Description = "Esta área reunirá os dados pessoais do usuário autenticado, ajustes de conta e informações da sessão atual.",
        PanelTitle = "Perfil do usuário",
        PanelDescription = "A página receberá dados do perfil, preferências e ações pessoais de administradores e professores."
    };

    public IActionResult OnGet() => PageOrLogin();
}
