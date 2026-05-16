namespace AlphaSquad.Shared.DTOs.PlatformTenants;

public record PlatformFeatureCatalogItemResponse(
    Guid Id,
    string Code,
    string Description
);

public record PlatformTenantAdminResponse(
    Guid UserId,
    string Name,
    string Email,
    bool IsActive,
    bool MustChangePassword,
    DateTime CreatedAt
);

public record PlatformTenantListItemResponse(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string PrimaryColor,
    string SecondaryColor,
    bool IsActive,
    DateTime CreatedAt,
    string PrimaryAdminName,
    string PrimaryAdminEmail,
    bool PrimaryAdminMustChangePassword,
    int FeatureCount
);

public record PlatformTenantDetailsResponse(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string PrimaryColor,
    string SecondaryColor,
    bool IsActive,
    DateTime CreatedAt,
    PlatformTenantAdminResponse? PrimaryAdmin,
    List<PlatformFeatureCatalogItemResponse> Features
);

public record CreatePlatformTenantRequest(
    string Name,
    string Slug,
    string? LogoUrl,
    string PrimaryColor,
    string SecondaryColor,
    bool IsActive,
    string AdminName,
    string AdminEmail,
    List<string> FeatureCodes
);

public record UpdatePlatformTenantRequest(
    string Name,
    string Slug,
    string? LogoUrl,
    string PrimaryColor,
    string SecondaryColor,
    bool IsActive,
    string AdminName,
    string AdminEmail,
    List<string> FeatureCodes
);

public record PlatformTenantProvisioningResponse(
    PlatformTenantDetailsResponse Tenant,
    string TemporaryPassword,
    bool MustChangePassword
);

public record PlatformTenantAdminPasswordResetResponse(
    Guid TenantId,
    Guid UserId,
    string Email,
    string TemporaryPassword,
    bool MustChangePassword
);
