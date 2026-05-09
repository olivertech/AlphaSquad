namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa o like simples de um usuario em uma publicacao da rede interna.
/// Nesta V1 o like e binario: o usuario curte ou remove a curtida.
/// </summary>
public class SocialPostLike
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid SocialPostId { get; set; }
    public SocialPost SocialPost { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
