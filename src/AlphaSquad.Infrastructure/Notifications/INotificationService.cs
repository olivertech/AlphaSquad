namespace AlphaSquad.Infrastructure.Notifications;

/// <summary>
/// Contrato central para publicar notificacoes institucionais do tenant.
/// A ideia e permitir que varios modulos emitam avisos sem duplicar regras de persistencia.
/// </summary>
public interface INotificationService
{
    Task<TenantNotification> PublishAsync(PublishTenantNotificationCommand command, CancellationToken cancellationToken = default);
}
