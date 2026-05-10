using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class TenantsService : ITenantsService
{
    private readonly IApiFacade _apiFacade;

    public TenantsService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task<TenantConfigResponseDto?> GETApiTenantsBySlugBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiTenantsBySlugBySlugAsync(slug, cancellationToken);
    }

    public Task<TenantCurrentResponseDto?> GETApiTenantsCurrentAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiTenantsCurrentAsync(cancellationToken);
    }

    public Task<TenantFeaturesResponseDto?> GETApiTenantsCurrentFeaturesAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiTenantsCurrentFeaturesAsync(cancellationToken);
    }

    public Task<TenantConfigResponseDto?> PUTApiTenantsByIdAsync(Guid id, UpdateTenantRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiTenantsByIdAsync(id, request, cancellationToken);
    }

    public Task<TenantCurrentResponseDto?> PUTApiTenantsCurrentLogoAsync(MultipartBodyDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiTenantsCurrentLogoAsync(request, cancellationToken);
    }
}