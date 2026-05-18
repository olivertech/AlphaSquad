namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Registra quando um usuario abriu ou marcou como lida uma notificacao do tenant.
/// Essa base sustenta a lista de pendencias e os filtros por lidas e nao lidas no app.
/// </summary>
public class UserNotificationRead
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantNotificationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public TenantNotification Notification { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
