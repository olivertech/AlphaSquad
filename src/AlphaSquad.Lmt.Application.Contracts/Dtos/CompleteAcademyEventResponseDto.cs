namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class CompleteAcademyEventResponseDto
{
    public int? AwardedParticipantsCount { get; set; }
    public Guid? EventId { get; set; }
    public string? EventTitle { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsCompleted { get; set; }
}
