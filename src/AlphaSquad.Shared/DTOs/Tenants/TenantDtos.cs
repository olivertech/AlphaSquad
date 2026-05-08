using static AlphaSquad.Shared.DTOs.Tenants.TenantFeaturesResponse;

namespace AlphaSquad.Shared.DTOs.Tenants;

public record TenantConfigResponse(
    Guid Id, 
    string Name, 
    string Slug, 
    string? LogoUrl, 
    string PrimaryColor, 
    string SecondaryColor, 
    bool IsActive
);

public record TenantCurrentResponse(
    Guid Id, 
    string Name, 
    string Slug, 
    string? LogoUrl, 
    string PrimaryColor, 
    string SecondaryColor, 
    bool IsActive
);

public record TenantFeaturesResponse(
    List<FeatureItem> Features
)
{
    public record FeatureItem(string Name, string Description);
}

public record UpdateTenantRequest(
    string Name, 
    string? LogoUrl, 
    string PrimaryColor, 
    string SecondaryColor
);

public record UpdateTenantLogoRequest(
    string FileName
);
