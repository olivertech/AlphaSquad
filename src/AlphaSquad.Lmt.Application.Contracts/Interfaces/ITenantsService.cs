using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface ITenantsService
{
    Task<TenantConfigResponseDto?> GETApiTenantsBySlugBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<TenantCurrentResponseDto?> GETApiTenantsCurrentAsync(CancellationToken cancellationToken = default);

    Task<TenantFeaturesResponseDto?> GETApiTenantsCurrentFeaturesAsync(CancellationToken cancellationToken = default);

    Task<TenantConfigResponseDto?> PUTApiTenantsByIdAsync(Guid id, UpdateTenantRequestDto request, CancellationToken cancellationToken = default);

    Task<TenantCurrentResponseDto?> PUTApiTenantsCurrentLogoAsync(MultipartBodyDto request, CancellationToken cancellationToken = default);
}