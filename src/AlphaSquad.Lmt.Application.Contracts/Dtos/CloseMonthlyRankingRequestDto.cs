namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class CloseMonthlyRankingRequestDto
{
    public string? FirstPlacePrize { get; set; }
    public int? Month { get; set; }
    public string? SecondPlacePrize { get; set; }
    public string? ThirdPlacePrize { get; set; }
    public int? Year { get; set; }
}