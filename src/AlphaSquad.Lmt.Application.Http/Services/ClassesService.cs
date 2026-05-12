using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class ClassesService : IClassesService
{
    private readonly IApiFacade _apiFacade;

    public ClassesService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiClassesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiClassesByIdAsync(id, cancellationToken);
    }

    public Task DELETEApiClassesByIdBookAsync(string id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiClassesByIdBookAsync(id, cancellationToken);
    }

    public Task DELETEApiClassesByIdBookingsByBookingIdAsync(string id, Guid bookingId, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiClassesByIdBookingsByBookingIdAsync(id, bookingId, cancellationToken);
    }

    public Task<List<GymClassResponseDto>?> GETApiClassesAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, bool? isActive, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiClassesAsync(dateFrom, dateTo, isActive, page, pageSize, cancellationToken);
    }

    public Task<List<ClassBookingManagementResponseDto>?> GETApiClassesBookingsByUserByUserIdAsync(Guid userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, bool? onlyActiveClasses, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiClassesBookingsByUserByUserIdAsync(userId, dateFrom, dateTo, onlyActiveClasses, cancellationToken);
    }

    public Task<GymClassResponseDto?> GETApiClassesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiClassesByIdAsync(id, cancellationToken);
    }

    public Task<List<ClassBookingManagementResponseDto>?> GETApiClassesByIdBookingsAsync(string id, bool? onlyActiveClasses, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiClassesByIdBookingsAsync(id, onlyActiveClasses, cancellationToken);
    }

    public Task<GymClassResponseDto?> POSTApiClassesAsync(CreateGymClassRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiClassesAsync(request, cancellationToken);
    }

    public Task<ClassBookingResponseDto?> POSTApiClassesByIdBookAsync(string id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiClassesByIdBookAsync(id, cancellationToken);
    }

    public Task<ClassBookingResponseDto?> POSTApiClassesByIdBookingsAsync(string id, CreateClassBookingForUserRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiClassesByIdBookingsAsync(id, request, cancellationToken);
    }

    public Task<GymClassResponseDto?> PUTApiClassesByIdAsync(Guid id, UpdateGymClassRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiClassesByIdAsync(id, request, cancellationToken);
    }
}
