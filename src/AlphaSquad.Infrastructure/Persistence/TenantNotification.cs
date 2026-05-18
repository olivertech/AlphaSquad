namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa uma notificacao institucional publicada para o ecossistema da academia.
/// Ela pode nascer de acoes administrativas, eventos, produtos, aulas ou outros modulos do tenant.
/// </summary>
public class TenantNotification
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public TenantNotificationType Type { get; set; }
    public TenantNotificationAudience Audience { get; set; } = TenantNotificationAudience.StudentsOnly;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? MediaId { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public TenantMedia? Media { get; set; }
    public AppUser? CreatedByUser { get; set; }
    public ICollection<UserNotificationRead> Reads { get; set; } = [];
}
