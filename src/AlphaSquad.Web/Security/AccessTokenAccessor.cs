using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Web.Security;

public sealed class AccessTokenAccessor(IHttpContextAccessor httpContextAccessor) : IAccessTokenAccessor
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = httpContextAccessor.HttpContext?.Session.GetString("access_token");
        return Task.FromResult(token);
    }
}
