using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class WorkoutsService : IWorkoutsService
{
    private readonly IApiFacade _apiFacade;

    public WorkoutsService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiWorkoutsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiWorkoutsByIdAsync(id, cancellationToken);
    }

    public Task<List<WorkoutResponseDto>?> GETApiWorkoutsAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiWorkoutsAsync(cancellationToken);
    }

    public Task<WorkoutDetailsResponseDto?> GETApiWorkoutsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiWorkoutsByIdAsync(id, cancellationToken);
    }

    public Task<WorkoutResponseDto?> POSTApiWorkoutsAsync(WorkoutCreateRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiWorkoutsAsync(request, cancellationToken);
    }

    public Task<WorkoutDetailsResponseDto?> POSTApiWorkoutsByIdExercisesAsync(string id, List<ExerciseAssignmentRequestDto> request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiWorkoutsByIdExercisesAsync(id, request, cancellationToken);
    }

    public Task<WorkoutResponseDto?> PUTApiWorkoutsByIdAsync(Guid id, WorkoutUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiWorkoutsByIdAsync(id, request, cancellationToken);
    }
}