namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class AcademyEventParticipationResponseDto
{
    public Guid? AcademyEventId { get; set; }
    public string? EventTitle { get; set; }
    public Guid? Id { get; set; }
    public DateTimeOffset? ParticipatedAt { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}