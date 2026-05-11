namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa um plano pronto para exibicao no dashboard.
/// </summary>
public sealed class PlanListItemViewModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string PriceLabel { get; init; }
    public required int DurationDays { get; init; }
    public required bool IsActive { get; init; }
    public required string StatusLabel { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}
