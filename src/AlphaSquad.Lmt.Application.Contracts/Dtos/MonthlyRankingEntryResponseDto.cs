namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class MonthlyRankingEntryResponseDto
{
    public int? Position { get; set; }
    public string? PrizeDescription { get; set; }
    public double? TotalPoints { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}