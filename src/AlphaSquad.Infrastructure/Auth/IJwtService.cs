namespace AlphaSquad.Infrastructure.Auth;

public interface IJwtService
{
    string GenerateAccessToken(AppUser user, Tenant tenant, out DateTime expiresAt);
}