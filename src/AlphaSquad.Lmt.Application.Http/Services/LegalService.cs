using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class LegalService : ILegalService
{
    private readonly IApiFacade _apiFacade;

    public LegalService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task<TenantLegalContentResponseDto?> GETApiLegalCurrentAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiLegalCurrentAsync(cancellationToken);
    }

    public Task<TenantLegalContentResponseDto?> PUTApiLegalCurrentAsync(UpdateTenantLegalContentRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiLegalCurrentAsync(request, cancellationToken);
    }
}