using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IApiFacade _apiFacade;

    public ProfileService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiProfileMePhotoAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiProfileMePhotoAsync(cancellationToken);
    }

    public Task<ProfileResponseDto?> GETApiProfileMeAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiProfileMeAsync(cancellationToken);
    }

    public Task<ProfileResponseDto?> PUTApiProfileMeAsync(UpdateProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiProfileMeAsync(request, cancellationToken);
    }

    public Task<ProfileResponseDto?> PUTApiProfileMeEmailAsync(UpdateProfileEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiProfileMeEmailAsync(request, cancellationToken);
    }

    public Task<string?> PUTApiProfileMePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiProfileMePasswordAsync(request, cancellationToken);
    }

    public Task<ProfileResponseDto?> PUTApiProfileMePhotoAsync(MultipartBodyDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiProfileMePhotoAsync(request, cancellationToken);
    }
}