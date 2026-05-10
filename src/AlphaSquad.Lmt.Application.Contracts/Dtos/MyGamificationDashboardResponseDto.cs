namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class MyGamificationDashboardResponseDto
{
    public int? CurrentMonth { get; set; }
    public int? CurrentMonthEventCount { get; set; }
    public double? CurrentMonthPoints { get; set; }
    public int? CurrentMonthPosition { get; set; }
    public int? CurrentYear { get; set; }
    public double? TotalAccumulatedPoints { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}