namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class MonthlyWinnerHistoryEntryResponseDto
{
    public DateTimeOffsetDto? GeneratedAt { get; set; }
    public int? Month { get; set; }
    public int? Position { get; set; }
    public string? PrizeDescription { get; set; }
    public double? TotalPoints { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public int? Year { get; set; }
}