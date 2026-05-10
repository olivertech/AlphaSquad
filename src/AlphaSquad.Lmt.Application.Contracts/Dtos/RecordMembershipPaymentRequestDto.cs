namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class RecordMembershipPaymentRequestDto
{
    public double? AmountPaid { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public Guid? UserId { get; set; }
}