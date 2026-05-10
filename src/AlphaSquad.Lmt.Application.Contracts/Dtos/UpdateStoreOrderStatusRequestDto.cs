namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateStoreOrderStatusRequestDto
{
    public string? AdminNotes { get; set; }
    public int? Status { get; set; }
}