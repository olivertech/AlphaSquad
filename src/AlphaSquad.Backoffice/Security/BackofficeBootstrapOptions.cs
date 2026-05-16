namespace AlphaSquad.Backoffice.Security;

/// <summary>
/// Define as credenciais locais usadas para bootstrap do backoffice enquanto a API master ainda nao existe.
/// </summary>
public sealed class BackofficeBootstrapOptions
{
    public string Name { get; set; } = "Equipe AlphaSquad";
    public string Email { get; set; } = "owner@alphasquad.app";
    public string Password { get; set; } = "AlphaSquad123!";
}
