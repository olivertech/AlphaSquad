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
    public async Task DELETEApiWorkoutsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        await _apiClient.Api.Workouts[id].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

    }

    public async Task<List<WorkoutResponseDto>?> GETApiWorkoutsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Workouts.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<WorkoutResponseDto>(result);

    }

    public async Task<WorkoutDetailsResponseDto?> GETApiWorkoutsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Workouts[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<WorkoutDetailsResponseDto>(result);

    }

    public async Task<WorkoutResponseDto?> POSTApiWorkoutsAsync(WorkoutCreateRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.WorkoutCreateRequest>(request);

        var result = await _apiClient.Api.Workouts.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<WorkoutResponseDto>(result);

    }

    public async Task<WorkoutDetailsResponseDto?> POSTApiWorkoutsByIdExercisesAsync(string id, List<ExerciseAssignmentRequestDto> request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<List<AlphaSquad.Lmt.Application.ApiClient.Models.ExerciseAssignmentRequest>>(request);

#pragma warning disable CS0618
        var result = await _apiClient.Api.Workouts[id].Exercises.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<WorkoutDetailsResponseDto>(result);

    }

    public async Task<WorkoutResponseDto?> PUTApiWorkoutsByIdAsync(Guid id, WorkoutUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.WorkoutUpdateRequest>(request);

#pragma warning disable CS0618
        var result = await _apiClient.Api.Workouts[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<WorkoutResponseDto>(result);

    }
}