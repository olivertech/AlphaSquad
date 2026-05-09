namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa uma publicacao feita por um usuario na rede interna da academia.
/// O feed e global por tenant, sem conceito de seguir outros usuarios nesta V1.
/// </summary>
public class SocialPost
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? MediaId { get; set; }
    public TenantMedia? Media { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<SocialPostLike> Likes { get; set; } = [];
    public ICollection<SocialPostComment> Comments { get; set; } = [];
}
