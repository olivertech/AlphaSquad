using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IPlansService
{
    Task<List<MembershipPlanResponseDto>?> GETApiPlansAsync(CancellationToken cancellationToken = default);

    Task<List<UserWithoutActivePlanResponseDto>?> GETApiPlansInactiveUsersAsync(int? daysWithoutPlan, CancellationToken cancellationToken = default);

    Task<List<UserMembershipHistoryResponseDto>?> GETApiPlansUsersByUserIdHistoryAsync(string userId, CancellationToken cancellationToken = default);

    Task<MembershipPlanResponseDto?> POSTApiPlansAsync(CreateMembershipPlanRequestDto request, CancellationToken cancellationToken = default);

    Task POSTApiPlansByIdAssignAsync(string id, AssignMembershipPlanRequestDto request, CancellationToken cancellationToken = default);

    Task<MembershipPaymentResponseDto?> POSTApiPlansPaymentsAsync(RecordMembershipPaymentRequestDto request, CancellationToken cancellationToken = default);

    Task<MembershipPlanResponseDto?> PUTApiPlansByIdAsync(Guid id, UpdateMembershipPlanRequestDto request, CancellationToken cancellationToken = default);
}