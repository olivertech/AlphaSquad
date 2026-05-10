namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ProductVariantResponseDto
{
    public string? Color { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
    public double? Price { get; set; }
    public Guid? ProductId { get; set; }
    public string? Size { get; set; }
    public int? StockQuantity { get; set; }
}