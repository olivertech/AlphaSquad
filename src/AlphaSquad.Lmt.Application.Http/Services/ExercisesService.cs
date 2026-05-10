using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class ExercisesService : IExercisesService
{
    private readonly IApiFacade _apiFacade;

    public ExercisesService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiExercisesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiExercisesByIdAsync(id, cancellationToken);
    }

    public Task<List<ExerciseResponseDto>?> GETApiExercisesAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiExercisesAsync(cancellationToken);
    }

    public Task<ExerciseResponseDto?> GETApiExercisesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiExercisesByIdAsync(id, cancellationToken);
    }

    public Task<ExerciseResponseDto?> POSTApiExercisesAsync(ExerciseCreateRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiExercisesAsync(request, cancellationToken);
    }

    public Task<ExerciseResponseDto?> PUTApiExercisesByIdAsync(Guid id, ExerciseUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiExercisesByIdAsync(id, request, cancellationToken);
    }
}