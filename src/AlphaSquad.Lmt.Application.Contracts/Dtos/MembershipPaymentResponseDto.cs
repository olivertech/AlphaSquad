namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class MembershipPaymentResponseDto
{
    public double? AmountPaid { get; set; }
    public DateTimeOffsetDto? CreatedAt { get; set; }
    public DateTimeOffsetDto? DueDate { get; set; }
    public Guid? Id { get; set; }
    public bool? IsPaidOnTime { get; set; }
    public Guid? MembershipPlanId { get; set; }
    public string? MembershipPlanName { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffsetDto? PaidAt { get; set; }
    public Guid? RecordedByUserId { get; set; }
    public string? RecordedByUserName { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}