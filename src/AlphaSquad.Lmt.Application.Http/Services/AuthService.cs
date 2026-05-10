using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class AuthService : IAuthService
{
    private readonly IApiFacade _apiFacade;

    public AuthService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task<AuthenticatedSessionResponseDto?> GETApiAuthMeAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiAuthMeAsync(cancellationToken);
    }

    public Task<string?> POSTApiAuthChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiAuthChangePasswordAsync(request, cancellationToken);
    }

    public Task<LoginResponseDto?> POSTApiAuthLoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiAuthLoginAsync(request, cancellationToken);
    }

    public Task POSTApiAuthLogoutAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiAuthLogoutAsync(cancellationToken);
    }

    public Task<LoginResponseDto?> POSTApiAuthRefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiAuthRefreshAsync(request, cancellationToken);
    }
}