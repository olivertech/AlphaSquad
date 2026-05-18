namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UserGamificationHistoryEntryResponseDto
{
    public Guid? Id { get; set; }
    public int? EventType { get; set; }
    public string? RuleName { get; set; }
    public double? PointsApplied { get; set; }
    public string? SourceEntity { get; set; }
    public string? SourceLabel { get; set; }
    public Guid? SourceEntityId { get; set; }
    public string? SourceTitle { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
    public string? Notes { get; set; }
}
