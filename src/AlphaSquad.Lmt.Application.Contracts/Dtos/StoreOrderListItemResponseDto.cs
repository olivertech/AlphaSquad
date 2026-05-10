namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class StoreOrderListItemResponseDto
{
    public DateTimeOffsetDto? CreatedAt { get; set; }
    public Guid? Id { get; set; }
    public int? Status { get; set; }
    public double? TotalAmount { get; set; }
    public int? TotalItems { get; set; }
    public DateTimeOffsetDto? UpdatedAt { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}