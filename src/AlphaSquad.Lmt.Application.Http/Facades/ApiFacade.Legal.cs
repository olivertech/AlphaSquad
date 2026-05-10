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
    public async Task<TenantLegalContentResponseDto?> GETApiLegalCurrentAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Legal.Current.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<TenantLegalContentResponseDto>(result);

    }

    public async Task<TenantLegalContentResponseDto?> PUTApiLegalCurrentAsync(UpdateTenantLegalContentRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateTenantLegalContentRequest>(request);

        var result = await _apiClient.Api.Legal.Current.PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<TenantLegalContentResponseDto>(result);

    }
}