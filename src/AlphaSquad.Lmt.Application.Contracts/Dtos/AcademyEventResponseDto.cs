namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class AcademyEventResponseDto
{
    public bool? AllowParticipation { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsOutdoorEvent { get; set; }
    public bool? IsUserParticipating { get; set; }
    public string? Location { get; set; }
    public Guid? MediaId { get; set; }
    public string? MediaUrl { get; set; }
    public int? ParticipantCount { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}