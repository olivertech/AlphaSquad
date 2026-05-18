namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class CreateUserRequestDto
{
    public string? BirthDate { get; set; }
    public string? Email { get; set; }
    public int? MembershipBillingDueDay { get; set; }
    public Guid? MembershipPlanId { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public int? Role { get; set; }
}
