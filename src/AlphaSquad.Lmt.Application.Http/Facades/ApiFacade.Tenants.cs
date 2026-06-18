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
    public async Task<TenantConfigResponseDto?> GETApiTenantsBySlugBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Tenants.BySlug[slug].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<TenantConfigResponseDto>(result);

    }

    public async Task<TenantCurrentResponseDto?> GETApiTenantsCurrentAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Tenants.Current.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<TenantCurrentResponseDto>(result);

    }

    public async Task<TenantFeaturesResponseDto?> GETApiTenantsCurrentFeaturesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Tenants.Current.Features.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<TenantFeaturesResponseDto>(result);

    }

    public async Task<TenantConfigResponseDto?> PUTApiTenantsByIdAsync(Guid id, UpdateTenantRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateTenantRequest>(request);

        var result = await _apiClient.Api.Tenants[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<TenantConfigResponseDto>(result);

    }

    public async Task<TenantCurrentResponseDto?> PUTApiTenantsCurrentLogoAsync(MultipartBodyDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = new MultipartBody();
        var fileStream = new MemoryStream(request.Content);
        kiotaRequest.AddOrReplacePart("file", request.ContentType, fileStream);

        var result = await _apiClient.Api.Tenants.Current.Logo.PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<TenantCurrentResponseDto>(result);

    }
}
