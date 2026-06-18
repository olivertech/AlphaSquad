namespace AlphaSquad.Shared.DTOs.Auth;

using AlphaSquad.Shared.DTOs.Common;

public record LoginRequest(
    string TenantSlug, 
    string Email, 
    string Password
);

public record LoginResponse(
    string AccessToken, 
    DateTime ExpiresAt, 
    string RefreshToken, 
    AuthenticatedUserResponse User, 
    AuthenticatedTenantResponse Tenant
);

public record RefreshRequest(
    string RefreshToken
);

public record AuthenticatedUserResponse(
    Guid Id, 
    string Name, 
    string Email, 
    UserRole Role,
    string? Username,
    string? ProfilePhotoUrl,
    Guid? ActivePlanId,
    string? ActivePlan,
    decimal? ActivePlanPrice,
    int? ActivePlanDurationDays,
    bool MustChangePassword
)
{
    public MediaPresentationResponse ProfilePhoto => MediaPresentationFactory.ForAvatar(Name, ProfilePhotoUrl);
}

public record AuthenticatedSessionResponse(
    Guid UserId,
    string Name,
    string Email,
    UserRole Role,
    string? Username,
    string? ProfilePhotoUrl,
    Guid? ActivePlanId,
    string? ActivePlan,
    decimal? ActivePlanPrice,
    int? ActivePlanDurationDays,
    bool MustChangePassword,
    Guid TenantId,
    string TenantSlug
)
{
    public MediaPresentationResponse ProfilePhoto => MediaPresentationFactory.ForAvatar(Name, ProfilePhotoUrl);
}

public record AuthenticatedTenantResponse(
    Guid Id, 
    string Name, 
    string Slug, 
    string? LogoUrl, 
    string PrimaryColor, 
    string SecondaryColor
)
{
    public MediaPresentationResponse Logo => MediaPresentationFactory.ForLogo(Name, LogoUrl);
}

public record ChangePasswordRequest(
    string CurrentPassword, 
    string NewPassword
);
