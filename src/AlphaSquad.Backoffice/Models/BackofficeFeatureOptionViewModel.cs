namespace AlphaSquad.Backoffice.Models;

/// <summary>
/// Representa uma feature opcional que pode ser liberada para a academia no onboarding.
/// </summary>
public sealed class BackofficeFeatureOptionViewModel
{
    public string Code { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
