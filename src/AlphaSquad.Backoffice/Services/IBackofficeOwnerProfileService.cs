using AlphaSquad.Shared.DTOs.PlatformProfile;

namespace AlphaSquad.Backoffice.Services;

public interface IBackofficeOwnerProfileService
{
    Task<PlatformProfileResponse> GetAsync(CancellationToken cancellationToken = default);
    Task<PlatformProfileResponse> UpdateAsync(string name, CancellationToken cancellationToken = default);
    Task<string> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<PlatformProfileResponse> UploadPhotoAsync(IFormFile photo, CancellationToken cancellationToken = default);
    Task<PlatformProfileResponse> DeletePhotoAsync(CancellationToken cancellationToken = default);
}
