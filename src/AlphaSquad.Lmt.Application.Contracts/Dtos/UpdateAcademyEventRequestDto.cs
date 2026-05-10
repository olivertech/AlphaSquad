namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateAcademyEventRequestDto
{
    public bool? AllowParticipation { get; set; }
    public string? Description { get; set; }
    public DateTimeOffsetDto? EndsAt { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsOutdoorEvent { get; set; }
    public string? Location { get; set; }
    public Guid? MediaId { get; set; }
    public DateTimeOffsetDto? StartsAt { get; set; }
    public string? Title { get; set; }
}