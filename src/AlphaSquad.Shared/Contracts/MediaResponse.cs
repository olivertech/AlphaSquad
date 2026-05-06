namespace AlphaSquad.Shared.Contracts;

/// <summary>
/// Resposta contendo os detalhes de um arquivo de mídia.
/// </summary>
public class MediaResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
