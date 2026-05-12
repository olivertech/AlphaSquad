using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IClassesService
{
    Task DELETEApiClassesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DELETEApiClassesByIdBookAsync(string id, CancellationToken cancellationToken = default);

    Task DELETEApiClassesByIdBookingsByBookingIdAsync(string id, Guid bookingId, CancellationToken cancellationToken = default);

    Task<List<GymClassResponseDto>?> GETApiClassesAsync(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, bool? isActive, int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<List<ClassBookingManagementResponseDto>?> GETApiClassesBookingsByUserByUserIdAsync(Guid userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, bool? onlyActiveClasses, CancellationToken cancellationToken = default);

    Task<GymClassResponseDto?> GETApiClassesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ClassBookingManagementResponseDto>?> GETApiClassesByIdBookingsAsync(string id, bool? onlyActiveClasses, CancellationToken cancellationToken = default);

    Task<GymClassResponseDto?> POSTApiClassesAsync(CreateGymClassRequestDto request, CancellationToken cancellationToken = default);

    Task<ClassBookingResponseDto?> POSTApiClassesByIdBookAsync(string id, CancellationToken cancellationToken = default);

    Task<ClassBookingResponseDto?> POSTApiClassesByIdBookingsAsync(string id, CreateClassBookingForUserRequestDto request, CancellationToken cancellationToken = default);

    Task<GymClassResponseDto?> PUTApiClassesByIdAsync(Guid id, UpdateGymClassRequestDto request, CancellationToken cancellationToken = default);
}
