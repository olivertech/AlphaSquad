using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IWorkoutsService
{
    Task DELETEApiWorkoutsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<WorkoutResponseDto>?> GETApiWorkoutsAsync(CancellationToken cancellationToken = default);

    Task<WorkoutDetailsResponseDto?> GETApiWorkoutsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkoutResponseDto?> POSTApiWorkoutsAsync(WorkoutCreateRequestDto request, CancellationToken cancellationToken = default);

    Task<WorkoutDetailsResponseDto?> POSTApiWorkoutsByIdExercisesAsync(string id, List<ExerciseAssignmentRequestDto> request, CancellationToken cancellationToken = default);

    Task<WorkoutResponseDto?> PUTApiWorkoutsByIdAsync(Guid id, WorkoutUpdateRequestDto request, CancellationToken cancellationToken = default);
}