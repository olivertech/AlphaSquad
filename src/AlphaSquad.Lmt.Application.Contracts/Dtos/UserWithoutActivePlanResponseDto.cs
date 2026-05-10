namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UserWithoutActivePlanResponseDto
{
    public int? DaysWithoutActivePlan { get; set; }
    public string? Email { get; set; }
    public DateTimeOffsetDto? LastPlanEndedAt { get; set; }
    public Guid? LastPlanId { get; set; }
    public string? LastPlanName { get; set; }
    public int? Role { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}