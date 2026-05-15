namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UploadMediaResponseDto
{
    public Guid? Id { get; set; }
    public string? FileName { get; set; }
    public string? Url { get; set; }
}
