namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UserMembershipHistoryResponseDto
{
    public Guid? ChangedByUserId { get; set; }
    public string? ChangedByUserName { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public int? DurationDays { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public Guid? MembershipPlanId { get; set; }
    public string? PlanName { get; set; }
    public double? PlanPrice { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public string? StatusReason { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}