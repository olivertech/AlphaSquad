namespace AlphaSquad.Shared.DTOs.Notifications;

public record NotificationListItemResponse(
    Guid Id,
    TenantNotificationType Type,
    TenantNotificationAudience Audience,
    string Title,
    string? Summary,
    Guid? MediaId,
    string? MediaUrl,
    bool IsHighlighted,
    DateTime PublishedAt,
    DateTime? ExpiresAt,
    bool IsRead,
    DateTime? ReadAt
);

public record NotificationDetailsResponse(
    Guid Id,
    TenantNotificationType Type,
    TenantNotificationAudience Audience,
    string Title,
    string? Summary,
    string Content,
    Guid? MediaId,
    string? MediaUrl,
    bool IsHighlighted,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTime PublishedAt,
    DateTime? ExpiresAt,
    bool IsRead,
    DateTime? ReadAt
);

public record AdminNotificationListItemResponse(
    Guid Id,
    TenantNotificationType Type,
    TenantNotificationAudience Audience,
    string Title,
    string? Summary,
    bool IsHighlighted,
    bool IsActive,
    DateTime PublishedAt,
    DateTime? ExpiresAt,
    int ReadCount,
    DateTime CreatedAt
);

public record CreateNotificationRequest(
    TenantNotificationType Type,
    TenantNotificationAudience Audience,
    string Title,
    string? Summary,
    string Content,
    Guid? MediaId,
    bool IsHighlighted,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTime? PublishedAt,
    DateTime? ExpiresAt
);

public record UpdateNotificationRequest(
    TenantNotificationType Type,
    TenantNotificationAudience Audience,
    string Title,
    string? Summary,
    string Content,
    Guid? MediaId,
    bool IsHighlighted,
    bool IsActive,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTime? PublishedAt,
    DateTime? ExpiresAt
);

public record NotificationReadResponse(
    Guid NotificationId,
    bool IsRead,
    DateTime ReadAt
);

public record UnreadNotificationCountResponse(
    int Count
);
