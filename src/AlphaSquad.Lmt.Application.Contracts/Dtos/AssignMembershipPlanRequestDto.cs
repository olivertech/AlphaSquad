namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class AssignMembershipPlanRequestDto
{
    public int? BillingDueDay { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public string? StatusReason { get; set; }
    public Guid? UserId { get; set; }
}
