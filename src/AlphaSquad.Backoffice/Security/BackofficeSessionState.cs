namespace AlphaSquad.Backoffice.Security;

/// <summary>
/// Consolida o contexto autenticado do backoffice em um unico objeto de sessao.
/// Isso deixa o sponsor desacoplado do contexto de tenant usado no dashboard das academias.
/// </summary>
public sealed class BackofficeSessionState
{
    public string? AccessToken { get; set; }
    public Guid? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
}
