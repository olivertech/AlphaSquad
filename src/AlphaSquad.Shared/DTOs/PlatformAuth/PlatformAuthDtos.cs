namespace AlphaSquad.Shared.DTOs.PlatformAuth;

public record PlatformLoginRequest(
    string Email,
    string Password
);

public record PlatformLoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    PlatformAuthenticatedUserResponse User
);

public record PlatformRefreshRequest(
    string RefreshToken
);

public record PlatformAuthenticatedUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool MustChangePassword,
    string? ProfilePhotoUrl
);

public record PlatformAuthenticatedSessionResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    bool MustChangePassword,
    string? ProfilePhotoUrl
);

public record PlatformChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
