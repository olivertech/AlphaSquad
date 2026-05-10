namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateMembershipPlanRequestDto
{
    public string? Description { get; set; }
    public int? DurationDays { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
    public double? Price { get; set; }
}