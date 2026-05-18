namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class GenerateBirthdayHighlightEventRequestDto
{
    public Guid? MediaId { get; set; }
    public DateTimeOffset? ReferenceDate { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public DateTimeOffset? HighlightStartsAt { get; set; }
    public DateTimeOffset? HighlightEndsAt { get; set; }
    public bool? IsActive { get; set; }
}
