namespace AlphaSquad.Shared.Helpers;

/// <summary>
/// Padroniza os valores das claims usadas para diferenciar o contexto master do contexto tenant.
/// </summary>
public static class PlatformClaimValues
{
    public const string Owner = "owner";
}
