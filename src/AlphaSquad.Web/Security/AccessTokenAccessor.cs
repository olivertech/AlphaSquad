using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Web.Security;

public sealed class AccessTokenAccessor(IHttpContextAccessor httpContextAccessor) : IAccessTokenAccessor
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // A camada HTTP gerada pela LMT consome o token da sessao web atual, sem expor JWT nas paginas.
        var session = httpContextAccessor.HttpContext?.Session.GetDashboardSession();
        return Task.FromResult(session?.AccessToken);
    }
}
