namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class AssignMembershipPlanRequestDto
{
    public DateTimeOffset? EndsAt { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public string? StatusReason { get; set; }
    public Guid? UserId { get; set; }
}