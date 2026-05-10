namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateProductVariantRequestDto
{
    public string? Color { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
    public double? Price { get; set; }
    public string? Size { get; set; }
    public int? StockQuantity { get; set; }
}