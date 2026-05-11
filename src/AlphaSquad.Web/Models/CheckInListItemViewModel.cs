namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa um registro de check-in pronto para exibicao no dashboard.
/// </summary>
public sealed class CheckInListItemViewModel
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required string UserRoleLabel { get; init; }
    public required string NotesPreview { get; init; }
    public DateTimeOffset? CheckedInAt { get; init; }
}
