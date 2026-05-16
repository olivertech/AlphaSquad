namespace AlphaSquad.Infrastructure.Auth;

/// <summary>
/// Representa a conta bootstrap usada para garantir o primeiro acesso ao backoffice da AlphaSquad.
/// Em produção, o recomendado é sobrescrever esses valores por secrets ou variáveis de ambiente.
/// </summary>
public sealed class PlatformBootstrapOptions
{
    public string Name { get; set; } = "Equipe AlphaSquad";
    public string Email { get; set; } = "owner@alphasquad.app";
    public string Password { get; set; } = "AlphaSquad123!";
}
