using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class PlansService : IPlansService
{
    private readonly IApiFacade _apiFacade;

    public PlansService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task<List<MembershipPlanResponseDto>?> GETApiPlansAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiPlansAsync(cancellationToken);
    }

    public Task<List<UserWithoutActivePlanResponseDto>?> GETApiPlansInactiveUsersAsync(int? daysWithoutPlan, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiPlansInactiveUsersAsync(daysWithoutPlan, cancellationToken);
    }

    public Task<List<UserMembershipHistoryResponseDto>?> GETApiPlansUsersByUserIdHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiPlansUsersByUserIdHistoryAsync(userId, cancellationToken);
    }

    public Task<MembershipPlanResponseDto?> POSTApiPlansAsync(CreateMembershipPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiPlansAsync(request, cancellationToken);
    }

    public Task POSTApiPlansByIdAssignAsync(string id, AssignMembershipPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiPlansByIdAssignAsync(id, request, cancellationToken);
    }

    public Task<MembershipPaymentResponseDto?> POSTApiPlansPaymentsAsync(RecordMembershipPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiPlansPaymentsAsync(request, cancellationToken);
    }

    public Task<MembershipPlanResponseDto?> PUTApiPlansByIdAsync(Guid id, UpdateMembershipPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiPlansByIdAsync(id, request, cancellationToken);
    }
}