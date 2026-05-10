using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class CheckinsService : ICheckinsService
{
    private readonly IApiFacade _apiFacade;

    public CheckinsService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task<List<UserWithoutRecentCheckInResponseDto>?> GETApiCheckinsInactiveUsersAsync(int? daysWithoutCheckIn, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiCheckinsInactiveUsersAsync(daysWithoutCheckIn, cancellationToken);
    }

    public Task<List<CheckInResponseDto>?> GETApiCheckinsMeAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiCheckinsMeAsync(dateFrom, dateTo, page, pageSize, cancellationToken);
    }

    public Task<List<CheckInResponseDto>?> GETApiCheckinsTenantAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int? page, int? pageSize, Guid? userId, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiCheckinsTenantAsync(dateFrom, dateTo, page, pageSize, userId, cancellationToken);
    }

    public Task<CheckInResponseDto?> POSTApiCheckinsAsync(CreateCheckInRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiCheckinsAsync(request, cancellationToken);
    }
}