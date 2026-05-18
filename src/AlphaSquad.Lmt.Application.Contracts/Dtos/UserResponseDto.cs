namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UserResponseDto
{
    public Guid? ActiveMembershipPlanId { get; set; }
    public string? ActiveMembershipPlanName { get; set; }
    public string? BirthDate { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? Email { get; set; }
    public Guid? Id { get; set; }
    public bool? IsGamificationParticipant { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsMembershipInGoodStanding { get; set; }
    public int? MembershipBillingDueDay { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public int? Role { get; set; }
    public Guid? TenantId { get; set; }
    public double? TotalAccumulatedPoints { get; set; }
}
