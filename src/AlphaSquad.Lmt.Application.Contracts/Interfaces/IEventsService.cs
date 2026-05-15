using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IEventsService
{
    Task DELETEApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<AcademyEventResponseDto>?> GETApiEventsAsync(bool? isActive, bool? onlyOutdoor, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<AcademyEventResponseDto?> GETApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<AcademyEventResponseDto>?> GETApiEventsFeedAsync(string cursor, int? limit, bool? onlyOutdoor, CancellationToken cancellationToken = default);
    Task<AcademyEventResponseDto?> POSTApiEventsAsync(CreateAcademyEventRequestDto request, CancellationToken cancellationToken = default);
    Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipateAsync(string id, CancellationToken cancellationToken = default);
    Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdCheckinAsync(Guid id, EventCheckInRequestDto request, CancellationToken cancellationToken = default);
    Task<CompleteAcademyEventResponseDto?> POSTApiEventsByIdCompleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AcademyEventResponseDto?> PUTApiEventsByIdAsync(Guid id, UpdateAcademyEventRequestDto request, CancellationToken cancellationToken = default);
    Task<List<AcademyEventParticipationResponseDto>?> GETApiEventsByIdParticipantsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipantsByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task DELETEApiEventsByIdParticipantsByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
