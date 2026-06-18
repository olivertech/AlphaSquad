using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Http.Mappers;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade
{
    public async Task DELETEApiUsersByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _apiClient.Api.Users[id].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    }

    public async Task<List<UserResponseDto>?> GETApiUsersAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Users.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<UserResponseDto>(result);

    }

    public async Task<UserResponseDto?> GETApiUsersByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Users[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<UserResponseDto>(result);

    }

    public async Task<UserGamificationHistoryResponseDto?> GETApiUsersByIdGamificationHistoryAsync(Guid id, int? limit = default, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var url = limit.HasValue
            ? $"api/users/{id}/gamification-history?limit={limit.Value}"
            : $"api/users/{id}/gamification-history";

        return await client.GetFromJsonAsync<UserGamificationHistoryResponseDto>(url, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserResponseDto?> POSTApiUsersAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateUserRequest>(request);

        var result = await _apiClient.Api.Users.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<UserResponseDto>(result);

    }

    public async Task<UserResponseDto?> PUTApiUsersByIdAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateUserRequest>(request);

        var result = await _apiClient.Api.Users[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<UserResponseDto>(result);

    }
}
