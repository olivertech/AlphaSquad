using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IProfileService
{
    Task DELETEApiProfileMePhotoAsync(CancellationToken cancellationToken = default);

    Task<ProfileResponseDto?> GETApiProfileMeAsync(CancellationToken cancellationToken = default);

    Task<ProfileResponseDto?> PUTApiProfileMeAsync(UpdateProfileRequestDto request, CancellationToken cancellationToken = default);

    Task<ProfileResponseDto?> PUTApiProfileMeEmailAsync(UpdateProfileEmailRequestDto request, CancellationToken cancellationToken = default);

    Task<string?> PUTApiProfileMePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default);

    Task<ProfileResponseDto?> PUTApiProfileMePhotoAsync(MultipartBodyDto request, CancellationToken cancellationToken = default);
}