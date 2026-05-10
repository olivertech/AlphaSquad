namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ProductListItemResponseDto
{
    public DateTimeOffset? CreatedAt { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public string? MainMediaUrl { get; set; }
    public string? Name { get; set; }
    public double? StartingPrice { get; set; }
}