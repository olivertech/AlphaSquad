namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Entidade de junção que vincula um Tenant a uma Feature específica.
/// Esta tabela permite o controle granular de quais funcionalidades 
/// cada academia possui habilitadas em seu plano.
/// </summary>
public class TenantFeature
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public Guid FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;
}
