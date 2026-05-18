namespace AlphaSquad.Infrastructure.Notifications;

/// <summary>
/// Publica notificacoes do tenant em uma base central, permitindo reaproveitamento por eventos,
/// produtos, aulas e informes institucionais sem acoplamento com o app.
/// </summary>
public sealed class NotificationService(AppDbContext db) : INotificationService
{
    public async Task<TenantNotification> PublishAsync(PublishTenantNotificationCommand command, CancellationToken cancellationToken = default)
    {
        var notification = new TenantNotification
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            Type = command.Type,
            Audience = command.Audience,
            Title = command.Title.Trim(),
            Summary = NormalizeOptional(command.Summary),
            Content = command.Content.Trim(),
            MediaId = command.MediaId,
            IsHighlighted = command.IsHighlighted,
            IsActive = true,
            RelatedEntityType = NormalizeOptional(command.RelatedEntityType),
            RelatedEntityId = command.RelatedEntityId,
            CreatedByUserId = command.CreatedByUserId,
            PublishedAt = command.PublishedAt ?? DateTime.UtcNow,
            ExpiresAt = command.ExpiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.TenantNotifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);

        return notification;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
