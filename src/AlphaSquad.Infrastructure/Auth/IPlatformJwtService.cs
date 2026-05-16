namespace AlphaSquad.Infrastructure.Auth;

/// <summary>
/// Gera os tokens JWT do backoffice master da AlphaSquad.
/// O token global nao leva claims de tenant para evitar confundir o contexto do sponsor com o das academias.
/// </summary>
public interface IPlatformJwtService
{
    string GenerateAccessToken(PlatformUser user, out DateTime expiresAt);
    string GenerateRefreshToken();
}
