namespace AlphaSquad.Shared.Contracts;

/// <summary>
/// Resposta contendo a lista de funcionalidades habilitadas para o Tenant.
/// </summary>
public class TenantFeaturesResponse
{
    public List<FeatureItem> Features { get; set; } = [];

    public class FeatureItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
