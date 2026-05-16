namespace AlphaSquad.Backoffice.Models;

/// <summary>
/// Define a moldura visual padrao das paginas internas do backoffice.
/// </summary>
public sealed class BackofficePageFrameModel
{
    public required string Section { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string PanelTitle { get; init; }
    public required string PanelDescription { get; init; }
}
