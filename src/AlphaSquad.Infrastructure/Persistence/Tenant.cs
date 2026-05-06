namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa uma academia (Tenant) no sistema AlphaSquad.
/// O sistema é multi-tenant, significando que cada Tenant opera de forma isolada.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// URL da logo do Tenant. Mantida para compatibilidade e performance de leitura.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Referência para a mídia que representa a logo do Tenant.
    /// Utilizado para recuperar a StorageKey e gerenciar a remoção do arquivo no storage.
    /// </summary>
    public Guid? LogoMediaId { get; set; }
    public TenantMedia? LogoMedia { get; set; }

    public string PrimaryColor { get; set; } = "#111827";
    public string SecondaryColor { get; set; } = "#2563EB";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Usuários vinculados a esta academia.
    /// </summary>
    public ICollection<AppUser> Users { get; set; } = [];
    
    /// <summary>
    /// Mídias e arquivos associados a esta academia.
    /// </summary>
    public ICollection<TenantMedia> Medias { get; set; } = [];

    /// <summary>
    /// Funcionalidades habilitadas para esta academia.
    /// </summary>
    public ICollection<TenantFeature> TenantFeatures { get; set; } = [];
}
