namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa uma funcionalidade disponível na plataforma AlphaSquad.
/// As features são usadas para controlar o acesso a módulos específicos do sistema
/// com base no plano ou configuração de cada Tenant.
/// </summary>
public class Feature
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Nome identificador da feature (ex: "WORKOUTS", "CHECKIN").
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição detalhada da funcionalidade para fins de documentação e interface.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Relacionamento com os Tenants que possuem esta feature habilitada.
    /// </summary>
    public ICollection<TenantFeature> TenantFeatures { get; set; } = [];
}
