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
    public async Task DELETEApiProfileMePhotoAsync(CancellationToken cancellationToken = default)
    {
        await _apiClient.Api.Profile.Me.Photo.DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    }

    public async Task<ProfileResponseDto?> GETApiProfileMeAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Profile.Me.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<ProfileResponseDto>(result);

    }

    public async Task<ProfileResponseDto?> PUTApiProfileMeAsync(UpdateProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateProfileRequest>(request);

        var result = await _apiClient.Api.Profile.Me.PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<ProfileResponseDto>(result);

    }

    public async Task<ProfileResponseDto?> PUTApiProfileMeEmailAsync(UpdateProfileEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateProfileEmailRequest>(request);

        var result = await _apiClient.Api.Profile.Me.Email.PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<ProfileResponseDto>(result);

    }

    public async Task<string?> PUTApiProfileMePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.ChangePasswordRequest>(request);

        var result = await _apiClient.Api.Profile.Me.Password.PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<string>(result);

    }

    public async Task<ProfileResponseDto?> PUTApiProfileMePhotoAsync(MultipartBodyDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<Microsoft.Kiota.Abstractions.MultipartBody>(request);

        var result = await _apiClient.Api.Profile.Me.Photo.PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<ProfileResponseDto>(result);

    }
}