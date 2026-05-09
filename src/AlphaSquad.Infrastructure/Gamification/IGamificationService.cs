namespace AlphaSquad.Infrastructure.Gamification;

/// <summary>
/// Contrato para processar eventos de gamificacao de forma centralizada.
/// O servico aplica regras, evita duplicidades e registra saldo de pontos do aluno.
/// </summary>
public interface IGamificationService
{
    Task<bool> AwardEventAsync(Guid tenantId,
                               Guid userId,
                               GamificationEventType eventType,
                               string sourceEntity,
                               Guid? sourceEntityId,
                               DateTime occurredAt,
                               string? notes = null);
}
