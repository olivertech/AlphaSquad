namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class MembershipPlanResponseDto
{
    public DateTimeOffsetDto? CreatedAt { get; set; }
    public string? Description { get; set; }
    public int? DurationDays { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
    public double? Price { get; set; }
}