using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Backoffice.Security;

/// <summary>
/// Mantem o projeto pronto para consumir APIs futuras do backoffice sem expor tokens nas paginas Razor.
/// </summary>
public sealed class AccessTokenAccessor(IHttpContextAccessor httpContextAccessor) : IAccessTokenAccessor
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var session = httpContextAccessor.HttpContext?.Session.GetBackofficeSession();
        return Task.FromResult(session?.AccessToken);
    }
}
