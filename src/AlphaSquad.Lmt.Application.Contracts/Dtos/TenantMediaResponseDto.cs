namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class TenantMediaResponseDto
{
    public string? ContentType { get; set; }
    public DateTimeOffsetDto? CreatedAt { get; set; }
    public string? FileName { get; set; }
    public Guid? Id { get; set; }
    public long? Size { get; set; }
    public Guid? TenantId { get; set; }
    public string? Url { get; set; }
}