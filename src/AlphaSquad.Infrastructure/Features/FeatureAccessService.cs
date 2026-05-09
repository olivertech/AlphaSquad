namespace AlphaSquad.Infrastructure.Features;

/// <summary>
/// Resolve o acesso a features opcionais usando a relacao entre tenant e feature.
/// Neste primeiro momento a verificacao vai direto ao banco, sem cache dedicado.
/// </summary>
public class FeatureAccessService : IFeatureAccessService
{
    private readonly AppDbContext _db;

    public FeatureAccessService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasFeatureAsync(Guid tenantId, string featureCode)
    {
        if (string.IsNullOrWhiteSpace(featureCode))
            return false;

        var normalizedFeatureCode = featureCode.Trim().ToUpperInvariant();

        return await _db.TenantFeatures.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.Feature.Name == normalizedFeatureCode);
    }
}
