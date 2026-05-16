namespace AlphaSquad.Backoffice.Navigation;

/// <summary>
/// Representa um item navegavel do menu do backoffice.
/// </summary>
public sealed class BackofficeMenuItem
{
    public string Label { get; init; } = string.Empty;
    public string? PagePath { get; init; }
    public string IconKey { get; init; } = "dashboard";

    public bool Matches(string? currentPage) =>
        !string.IsNullOrWhiteSpace(PagePath) &&
        string.Equals(PagePath, currentPage, StringComparison.OrdinalIgnoreCase);
}
