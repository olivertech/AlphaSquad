namespace AlphaSquad.Infrastructure.Auth;

using AlphaSquad.Infrastructure.Persistence;

public interface IJwtService
{
    string GenerateAccessToken(AppUser user, Tenant tenant, out DateTime expiresAt);
    string GenerateRefreshToken();
}
