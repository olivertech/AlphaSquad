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
    public async Task DELETEApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        await _apiClient.Api.Events[id].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618
    }

    public async Task<List<AcademyEventResponseDto>?> GETApiEventsAsync(bool? isActive, bool? onlyOutdoor, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Events.GetAsync(config =>
        {
            config.QueryParameters.IsActive = isActive;
            config.QueryParameters.OnlyOutdoor = onlyOutdoor;
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<AcademyEventResponseDto>(result?.Items);
    }

    public async Task<AcademyEventResponseDto?> GETApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Events[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<AcademyEventResponseDto>(result);
    }

    public async Task<List<AcademyEventResponseDto>?> GETApiEventsFeedAsync(string cursor, int? limit, bool? onlyOutdoor, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Events.Feed.GetAsync(config =>
        {
            config.QueryParameters.Cursor = cursor;
            config.QueryParameters.Limit = limit;
            config.QueryParameters.OnlyOutdoor = onlyOutdoor;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<AcademyEventResponseDto>(result?.Items);
    }

    public async Task<AcademyEventResponseDto?> POSTApiEventsAsync(CreateAcademyEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateAcademyEventRequest>(request);
        var result = await _apiClient.Api.Events.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
        return GeneratedDtoMapper.Map<AcademyEventResponseDto>(result);
    }

    public async Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipateAsync(string id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Events[id].Participate.PostAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<AcademyEventParticipationResponseDto>(result);
    }

    public async Task<AcademyEventResponseDto?> PUTApiEventsByIdAsync(Guid id, UpdateAcademyEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateAcademyEventRequest>(request);
#pragma warning disable CS0618
        var result = await _apiClient.Api.Events[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<AcademyEventResponseDto>(result);
    }
    public async Task<List<AcademyEventParticipationResponseDto>?> GETApiEventsByIdParticipantsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Events[id].Participants.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return GeneratedDtoMapper.MapList<AcademyEventParticipationResponseDto>(result);
    }

    public async Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipantsByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Events[id].Participants[userId].PostAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return GeneratedDtoMapper.Map<AcademyEventParticipationResponseDto>(result);
    }

    public async Task DELETEApiEventsByIdParticipantsByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        await _apiClient.Api.Events[id].Participants[userId].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
