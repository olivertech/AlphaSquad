namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class StoreOrderItemResponseDto
{
    public Guid? Id { get; set; }
    public double? LineTotal { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int? Quantity { get; set; }
    public double? UnitPrice { get; set; }
    public string? VariantColor { get; set; }
    public string? VariantName { get; set; }
    public string? VariantSize { get; set; }
}