namespace AlphaSquad.Backoffice.Navigation;

/// <summary>
/// Agrupa itens do menu em secoes de navegacao do backoffice.
/// </summary>
public sealed class BackofficeMenuSection
{
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<BackofficeMenuItem> Items { get; init; } = [];
}
