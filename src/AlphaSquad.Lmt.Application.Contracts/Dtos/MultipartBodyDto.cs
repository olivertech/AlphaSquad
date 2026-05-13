namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

/// <summary>
/// DTO para transporte de dados de arquivos em requisições multipart.
/// </summary>
public sealed class MultipartBodyDto
{
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
