namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa um plano disponível para seleção no formulário do usuário.
/// </summary>
public sealed class MembershipPlanOptionViewModel
{
    public required Guid Id { get; init; }
    public required string Label { get; init; }
    public required bool IsActive { get; init; }
}
