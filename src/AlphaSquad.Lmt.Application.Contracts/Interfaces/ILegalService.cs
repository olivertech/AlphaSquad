using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface ILegalService
{
    Task<TenantLegalContentResponseDto?> GETApiLegalCurrentAsync(CancellationToken cancellationToken = default);

    Task<TenantLegalContentResponseDto?> PUTApiLegalCurrentAsync(UpdateTenantLegalContentRequestDto request, CancellationToken cancellationToken = default);
}