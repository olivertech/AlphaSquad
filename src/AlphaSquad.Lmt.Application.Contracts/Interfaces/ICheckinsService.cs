using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface ICheckinsService
{
    Task<List<UserWithoutRecentCheckInResponseDto>?> GETApiCheckinsInactiveUsersAsync(int? daysWithoutCheckIn, CancellationToken cancellationToken = default);

    Task<List<CheckInResponseDto>?> GETApiCheckinsMeAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<List<CheckInResponseDto>?> GETApiCheckinsTenantAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int? page, int? pageSize, Guid? userId, CancellationToken cancellationToken = default);

    Task<CheckInResponseDto?> POSTApiCheckinsAsync(CreateCheckInRequestDto request, CancellationToken cancellationToken = default);
}