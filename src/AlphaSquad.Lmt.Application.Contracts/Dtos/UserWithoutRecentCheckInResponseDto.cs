namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UserWithoutRecentCheckInResponseDto
{
    public Guid? ActivePlanId { get; set; }
    public string? ActivePlanName { get; set; }
    public int? DaysWithoutCheckIn { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset? LastCheckedInAt { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public int? UserRole { get; set; }
}