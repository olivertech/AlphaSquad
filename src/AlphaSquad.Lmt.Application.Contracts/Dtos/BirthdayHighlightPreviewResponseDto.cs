namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class BirthdayHighlightPreviewResponseDto
{
    public DateTimeOffset? ReferenceDate { get; set; }
    public int? BirthdayCount { get; set; }
    public List<string> StudentNames { get; set; } = [];
    public bool? CanGenerateHighlight { get; set; }
}
