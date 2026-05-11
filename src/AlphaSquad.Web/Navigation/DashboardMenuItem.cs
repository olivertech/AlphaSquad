namespace AlphaSquad.Web.Navigation;

/// <summary>
/// Representa um item visual do menu lateral do dashboard.
/// Um item pode apontar para uma página final ou funcionar como agrupador de subitens.
/// </summary>
public sealed class DashboardMenuItem
{
    public required string Label { get; init; }
    public string? PagePath { get; init; }
    public string? IconKey { get; init; }
    public string? FeatureCode { get; init; }
    public IReadOnlyList<string> AllowedRoles { get; init; } = [];
    public IReadOnlyList<DashboardMenuItem> Children { get; init; } = [];

    /// <summary>
    /// Indica se o item é um agrupador visual sem destino final próprio.
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <summary>
    /// Identifica se o item atual ou algum subitem corresponde à página aberta no momento.
    /// Isso ajuda a marcar o menu ativo e a manter o submenu expandido.
    /// </summary>
    public bool Matches(string? currentPage)
    {
        if (!string.IsNullOrWhiteSpace(PagePath) &&
            string.Equals(PagePath, currentPage, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Children.Any(child => child.Matches(currentPage));
    }
}
