namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class AssignMembershipPlanRequestDto
{
    public DateTimeOffsetDto? EndsAt { get; set; }
    public DateTimeOffsetDto? StartsAt { get; set; }
    public string? StatusReason { get; set; }
    public Guid? UserId { get; set; }
}