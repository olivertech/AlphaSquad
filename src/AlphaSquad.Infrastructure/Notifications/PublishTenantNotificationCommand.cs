namespace AlphaSquad.Infrastructure.Notifications;

/// <summary>
/// Comando usado para publicar notificacoes gerais do tenant.
/// Ele reaproveita o mesmo modelo para avisos institucionais e para notificacoes disparadas por modulos.
/// </summary>
public sealed record PublishTenantNotificationCommand(
    Guid TenantId,
    TenantNotificationType Type,
    TenantNotificationAudience Audience,
    string Title,
    string? Summary,
    string Content,
    Guid? MediaId,
    bool IsHighlighted,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    Guid? CreatedByUserId,
    DateTime? PublishedAt,
    DateTime? ExpiresAt
);
