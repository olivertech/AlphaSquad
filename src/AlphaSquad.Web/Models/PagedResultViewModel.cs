namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa uma pagina local de resultados no dashboard.
/// Esse modelo ajuda a paginar listas do front mesmo quando a API ainda nao expõe totalizadores completos.
/// </summary>
public sealed class PagedResultViewModel<TItem>
{
    public required IReadOnlyList<TItem> Items { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required int TotalItems { get; init; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalItems / PageSize));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
