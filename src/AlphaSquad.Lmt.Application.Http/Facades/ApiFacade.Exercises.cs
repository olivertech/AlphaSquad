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
    public async Task DELETEApiExercisesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _apiClient.Api.Exercises[id].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    }

    public async Task<List<ExerciseResponseDto>?> GETApiExercisesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Exercises.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<ExerciseResponseDto>(result);

    }

    public async Task<ExerciseResponseDto?> GETApiExercisesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Exercises[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<ExerciseResponseDto>(result);

    }

    public async Task<ExerciseResponseDto?> POSTApiExercisesAsync(ExerciseCreateRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.ExerciseCreateRequest>(request);

        var result = await _apiClient.Api.Exercises.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<ExerciseResponseDto>(result);

    }

    public async Task<ExerciseResponseDto?> PUTApiExercisesByIdAsync(Guid id, ExerciseUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.ExerciseUpdateRequest>(request);

        var result = await _apiClient.Api.Exercises[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<ExerciseResponseDto>(result);

    }
}
