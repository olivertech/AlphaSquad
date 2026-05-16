namespace AlphaSquad.Backoffice.Navigation;

/// <summary>
/// Define a navegacao principal do backoffice da AlphaSquad.
/// </summary>
public sealed class BackofficeNavigationService : IBackofficeNavigationService
{
    private static readonly IReadOnlyList<BackofficeMenuSection> Sections =
    [
        new BackofficeMenuSection
        {
            Title = "Visao geral",
            Items =
            [
                new BackofficeMenuItem
                {
                    Label = "Dashboard",
                    PagePath = "/Backoffice/Index",
                    IconKey = "dashboard"
                }
            ]
        },
        new BackofficeMenuSection
        {
            Title = "Operacao da plataforma",
            Items =
            [
                new BackofficeMenuItem
                {
                    Label = "Academias",
                    PagePath = "/Backoffice/Tenants/Index",
                    IconKey = "academy"
                }
            ]
        }
    ];

    public IReadOnlyList<BackofficeMenuSection> Build() => Sections;
}
