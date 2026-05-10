namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class RecordMembershipPaymentRequestDto
{
    public double? AmountPaid { get; set; }
    public DateTimeOffsetDto? DueDate { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffsetDto? PaidAt { get; set; }
    public Guid? UserId { get; set; }
}