namespace AlphaSquad.Infrastructure.Persistence;

public class TenantMedia
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relação com Tenant
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
