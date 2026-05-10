using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class MediaService : IMediaService
{
    private readonly IApiFacade _apiFacade;

    public MediaService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiMediaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiMediaByIdAsync(id, cancellationToken);
    }

    public Task<List<TenantMediaResponseDto>?> GETApiMediaAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiMediaAsync(page, pageSize, cancellationToken);
    }

    public Task<MediaResponseDto?> GETApiMediaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiMediaByIdAsync(id, cancellationToken);
    }

    public Task<UploadMediaResponseDto?> POSTApiMediaUploadAsync(MultipartBodyDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiMediaUploadAsync(request, cancellationToken);
    }

    public Task<TenantMediaResponseDto?> PUTApiMediaByIdFileAsync(string id, MultipartBodyDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiMediaByIdFileAsync(id, request, cancellationToken);
    }
}