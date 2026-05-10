namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class AuthenticatedUserResponseDto
{
    public string? ActivePlan { get; set; }
    public int? ActivePlanDurationDays { get; set; }
    public Guid? ActivePlanId { get; set; }
    public double? ActivePlanPrice { get; set; }
    public string? Email { get; set; }
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public int? Role { get; set; }
    public string? Username { get; set; }
}