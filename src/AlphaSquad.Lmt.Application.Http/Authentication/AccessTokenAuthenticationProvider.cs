using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace AlphaSquad.Lmt.Application.Http.Authentication;

internal sealed class AccessTokenAuthenticationProvider : IAuthenticationProvider
{
    private readonly IAccessTokenAccessor _accessTokenAccessor;

    public AccessTokenAuthenticationProvider(IAccessTokenAccessor accessTokenAccessor)
    {
        _accessTokenAccessor = accessTokenAccessor ?? throw new ArgumentNullException(nameof(accessTokenAccessor));
    }

    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var accessToken = await _accessTokenAccessor.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        request.Headers.TryAdd("Authorization", $"Bearer {accessToken}");
    }
}