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
    public async Task<List<MembershipPlanResponseDto>?> GETApiPlansAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Plans.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<MembershipPlanResponseDto>(result);

    }

    public async Task<List<UserWithoutActivePlanResponseDto>?> GETApiPlansInactiveUsersAsync(int? daysWithoutPlan, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Plans.InactiveUsers.GetAsync(config =>
        {
            config.QueryParameters.DaysWithoutPlan = daysWithoutPlan;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<UserWithoutActivePlanResponseDto>(result);

    }

    public async Task<List<UserMembershipHistoryResponseDto>?> GETApiPlansUsersByUserIdHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Plans.Users[userId].History.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.MapList<UserMembershipHistoryResponseDto>(result);

    }

    public async Task<MembershipPlanResponseDto?> POSTApiPlansAsync(CreateMembershipPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateMembershipPlanRequest>(request);

        var result = await _apiClient.Api.Plans.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<MembershipPlanResponseDto>(result);

    }

    public async Task POSTApiPlansByIdAssignAsync(string id, AssignMembershipPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.AssignMembershipPlanRequest>(request);

#pragma warning disable CS0618
        await _apiClient.Api.Plans[id].Assign.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

    }

    public async Task<MembershipPaymentResponseDto?> POSTApiPlansPaymentsAsync(RecordMembershipPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.RecordMembershipPaymentRequest>(request);

        var result = await _apiClient.Api.Plans.Payments.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<MembershipPaymentResponseDto>(result);

    }

    public async Task<MembershipPlanResponseDto?> PUTApiPlansByIdAsync(Guid id, UpdateMembershipPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateMembershipPlanRequest>(request);

#pragma warning disable CS0618
        var result = await _apiClient.Api.Plans[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<MembershipPlanResponseDto>(result);

    }
}