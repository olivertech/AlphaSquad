using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IExercisesService
{
    Task DELETEApiExercisesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ExerciseResponseDto>?> GETApiExercisesAsync(CancellationToken cancellationToken = default);

    Task<ExerciseResponseDto?> GETApiExercisesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ExerciseResponseDto?> POSTApiExercisesAsync(ExerciseCreateRequestDto request, CancellationToken cancellationToken = default);

    Task<ExerciseResponseDto?> PUTApiExercisesByIdAsync(Guid id, ExerciseUpdateRequestDto request, CancellationToken cancellationToken = default);
}