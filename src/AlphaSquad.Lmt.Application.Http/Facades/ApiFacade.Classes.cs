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
    public async Task DELETEApiClassesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        await _apiClient.Api.Classes[id].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

    }

    public async Task DELETEApiClassesByIdBookAsync(string id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        await _apiClient.Api.Classes[id].Book.DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

    }

    public async Task<List<GymClassResponseDto>?> GETApiClassesAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, bool? isActive, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Classes.GetAsync(config =>
        {
            config.QueryParameters.DateFrom = dateFrom;
            config.QueryParameters.DateTo = dateTo;
            config.QueryParameters.IsActive = isActive;
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<GymClassResponseDto>(result?.Items);

    }

    public async Task<List<ClassBookingManagementResponseDto>?> GETApiClassesBookingsByUserByUserIdAsync(Guid userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, bool? onlyActiveClasses, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Classes.Bookings.ByUser[userId].GetAsync(config =>
        {
            config.QueryParameters.DateFrom = dateFrom;
            config.QueryParameters.DateTo = dateTo;
            config.QueryParameters.OnlyActiveClasses = onlyActiveClasses;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.MapList<ClassBookingManagementResponseDto>(result);

    }

    public async Task<GymClassResponseDto?> GETApiClassesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Classes[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<GymClassResponseDto>(result);

    }

    public async Task<List<ClassBookingManagementResponseDto>?> GETApiClassesByIdBookingsAsync(string id, bool? onlyActiveClasses, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Classes[id].Bookings.GetAsync(config =>
        {
            config.QueryParameters.OnlyActiveClasses = onlyActiveClasses;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.MapList<ClassBookingManagementResponseDto>(result);

    }

    public async Task<GymClassResponseDto?> POSTApiClassesAsync(CreateGymClassRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateGymClassRequest>(request);

        var result = await _apiClient.Api.Classes.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<GymClassResponseDto>(result);

    }

    public async Task<ClassBookingResponseDto?> POSTApiClassesByIdBookAsync(string id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Classes[id].Book.PostAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<ClassBookingResponseDto>(result);

    }

    public async Task<GymClassResponseDto?> PUTApiClassesByIdAsync(Guid id, UpdateGymClassRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateGymClassRequest>(request);

#pragma warning disable CS0618
        var result = await _apiClient.Api.Classes[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<GymClassResponseDto>(result);

    }
}