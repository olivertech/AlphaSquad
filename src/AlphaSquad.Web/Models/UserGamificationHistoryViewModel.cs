namespace AlphaSquad.Web.Models;

public sealed class UserGamificationHistoryViewModel
{
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required bool IsGamificationParticipant { get; init; }
    public decimal TotalAccumulatedPoints { get; init; }
    public required string SummaryText { get; init; }
    public IReadOnlyList<UserGamificationHistoryEntryViewModel> Entries { get; init; } = [];
}
