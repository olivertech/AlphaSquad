namespace AlphaSquad.Shared.DTOs.PlatformProfile;

public record PlatformProfileResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    bool MustChangePassword,
    string? ProfilePhotoUrl
);

public record UpdatePlatformProfileRequest(
    string Name
);
