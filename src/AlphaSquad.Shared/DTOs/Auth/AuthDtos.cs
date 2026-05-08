namespace AlphaSquad.Shared.DTOs.Auth;

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
    UserRole Role
);

public record AuthenticatedTenantResponse(
    Guid Id, 
    string Name, 
    string Slug, 
    string? LogoUrl, 
    string PrimaryColor, 
    string SecondaryColor
);

public record ChangePasswordRequest(
    string CurrentPassword, 
    string NewPassword
);
