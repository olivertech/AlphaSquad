namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um comentario simples em uma publicacao social.
/// O modelo nao suporta respostas encadeadas nesta primeira versao.
/// </summary>
public class SocialPostComment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid SocialPostId { get; set; }
    public SocialPost SocialPost { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
