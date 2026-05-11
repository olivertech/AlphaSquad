namespace AlphaSquad.Web.Models;

/// <summary>
/// Resume um aluno que esta sem fazer check-in ha alguns dias.
/// </summary>
public sealed class InactiveCheckInUserViewModel
{
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string DaysWithoutCheckInLabel { get; init; }
    public string? ActivePlanName { get; init; }
    public DateTimeOffset? LastCheckedInAt { get; init; }
}
