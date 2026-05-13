using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class EventsService : IEventsService
{
    private readonly IApiFacade _apiFacade;

    public EventsService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiEventsByIdAsync(id, cancellationToken);
    }

    public Task<List<AcademyEventResponseDto>?> GETApiEventsAsync(bool? isActive, bool? onlyOutdoor, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiEventsAsync(isActive, onlyOutdoor, page, pageSize, cancellationToken);
    }

    public Task<AcademyEventResponseDto?> GETApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiEventsByIdAsync(id, cancellationToken);
    }

    public Task<List<AcademyEventResponseDto>?> GETApiEventsFeedAsync(string cursor, int? limit, bool? onlyOutdoor, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiEventsFeedAsync(cursor, limit, onlyOutdoor, cancellationToken);
    }

    public Task<AcademyEventResponseDto?> POSTApiEventsAsync(CreateAcademyEventRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiEventsAsync(request, cancellationToken);
    }

    public Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipateAsync(string id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiEventsByIdParticipateAsync(id, cancellationToken);
    }

    public Task<AcademyEventResponseDto?> PUTApiEventsByIdAsync(Guid id, UpdateAcademyEventRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiEventsByIdAsync(id, request, cancellationToken);
    }

}
