namespace AlphaSquad.Web.Models;

public sealed class UserGamificationHistoryEntryViewModel
{
    public required Guid Id { get; init; }
    public required string EventTypeLabel { get; init; }
    public required string RuleName { get; init; }
    public required decimal PointsApplied { get; init; }
    public required string SourceLabel { get; init; }
    public string? SourceTitle { get; init; }
    public DateTimeOffset? OccurredAt { get; init; }
    public string? Notes { get; init; }
}
