namespace AlphaSquad.Web.Models;

/// <summary>
/// Resume um aluno que esta sem plano ativo, facilitando a leitura de retencao no dashboard.
/// </summary>
public sealed class InactivePlanUserViewModel
{
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string DaysWithoutPlanLabel { get; init; }
    public string? LastPlanName { get; init; }
    public DateTimeOffset? LastPlanEndedAt { get; init; }
}
