using AlphaSquad.Shared.DTOs.PlatformAuth;

namespace AlphaSquad.Backoffice.Services;

public interface IBackofficeAuthService
{
    Task<PlatformLoginResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<PlatformAuthenticatedSessionResponse> GetSessionAsync(CancellationToken cancellationToken = default);
}
