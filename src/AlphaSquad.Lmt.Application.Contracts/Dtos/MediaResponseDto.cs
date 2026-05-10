namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class MediaResponseDto
{
    public string? ContentType { get; set; }
    public string? FileName { get; set; }
    public Guid? Id { get; set; }
    public string? Url { get; set; }
}