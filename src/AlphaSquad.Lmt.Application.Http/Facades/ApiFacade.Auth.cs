using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Http.Mappers;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade
{
    public async Task<AuthenticatedSessionResponseDto?> GETApiAuthMeAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Auth.Me.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<AuthenticatedSessionResponseDto>(result);

    }

    public async Task<string?> POSTApiAuthChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.ChangePasswordRequest>(request);

        var result = await _apiClient.Api.Auth.ChangePassword.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<string>(result);

    }

    public async Task<LoginResponseDto?> POSTApiAuthLoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.LoginRequest>(request);

        var result = await _apiClient.Api.Auth.Login.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<LoginResponseDto>(result);

    }

    public async Task POSTApiAuthLogoutAsync(CancellationToken cancellationToken = default)
    {
        await _apiClient.Api.Auth.Logout.PostAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    }

    public async Task<LoginResponseDto?> POSTApiAuthRefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.RefreshRequest>(request);

        var result = await _apiClient.Api.Auth.Refresh.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<LoginResponseDto>(result);

    }
}