namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateUserRequestDto
{
    public string? BirthDate { get; set; }
    public bool? IsActive { get; set; }
    public int? MembershipBillingDueDay { get; set; }
    public Guid? MembershipPlanId { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public int? Role { get; set; }
}
