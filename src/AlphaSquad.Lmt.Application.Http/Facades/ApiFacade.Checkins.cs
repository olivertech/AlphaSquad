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
    public async Task<List<UserWithoutRecentCheckInResponseDto>?> GETApiCheckinsInactiveUsersAsync(int? daysWithoutCheckIn, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Checkins.InactiveUsers.GetAsync(config =>
        {
            config.QueryParameters.DaysWithoutCheckIn = daysWithoutCheckIn;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<UserWithoutRecentCheckInResponseDto>(result);

    }

    public async Task<List<CheckInResponseDto>?> GETApiCheckinsMeAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Checkins.Me.GetAsync(config =>
        {
            config.QueryParameters.DateFrom = dateFrom;
            config.QueryParameters.DateTo = dateTo;
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<CheckInResponseDto>(result?.Items);

    }

    public async Task<List<CheckInResponseDto>?> GETApiCheckinsTenantAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int? page, int? pageSize, Guid? userId, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Checkins.Tenant.GetAsync(config =>
        {
            config.QueryParameters.DateFrom = dateFrom;
            config.QueryParameters.DateTo = dateTo;
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
            config.QueryParameters.UserId = userId;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<CheckInResponseDto>(result?.Items);

    }

    public async Task<CheckInResponseDto?> POSTApiCheckinsAsync(CreateCheckInRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateCheckInRequest>(request);

        var result = await _apiClient.Api.Checkins.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<CheckInResponseDto>(result);

    }
}