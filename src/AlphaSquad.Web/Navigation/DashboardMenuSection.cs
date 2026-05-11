namespace AlphaSquad.Web.Navigation;

/// <summary>
/// Organiza o menu lateral em blocos menores e mais fáceis de explorar.
/// Isso deixa a navegação mais amigável para o dia a dia da academia.
/// </summary>
public sealed class DashboardMenuSection
{
    public required string Title { get; init; }
    public required IReadOnlyList<DashboardMenuItem> Items { get; init; }
}
