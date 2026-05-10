namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ProductDetailResponseDto
{
    public DateTimeOffsetDto? CreatedAt { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public Guid? MainMediaId { get; set; }
    public string? MainMediaUrl { get; set; }
    public string? Name { get; set; }
}