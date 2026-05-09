namespace AlphaSquad.Infrastructure.Gamification;

/// <summary>
/// Implementa o processamento central de pontuacao do aluno.
/// O servico garante que apenas alunos pontuem e que o mesmo fato nao gere pontos duplicados.
/// </summary>
public class GamificationService : IGamificationService
{
    private readonly AppDbContext _db;

    public GamificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> AwardEventAsync(Guid tenantId,
                                            Guid userId,
                                            GamificationEventType eventType,
                                            string sourceEntity,
                                            Guid? sourceEntityId,
                                            DateTime occurredAt,
                                            string? notes = null)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null || user.Role != UserRole.Student)
            return false;

        var rule = await _db.GamificationEventRules
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EventType == eventType && x.IsActive);

        if (rule is null)
            return false;

        var alreadyExists = await _db.UserGamificationEvents.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.UserId == userId &&
            x.EventType == eventType &&
            x.SourceEntity == sourceEntity &&
            x.SourceEntityId == sourceEntityId);

        if (alreadyExists)
            return false;

        var lastBalance = await _db.PointsLedgers
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => (decimal?)x.BalanceAfter)
            .FirstOrDefaultAsync() ?? 0m;

        var gamificationEvent = new UserGamificationEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            GamificationEventRuleId = rule.Id,
            EventType = eventType,
            SourceEntity = sourceEntity,
            SourceEntityId = sourceEntityId,
            PointsApplied = rule.Points,
            OccurredAt = occurredAt,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.UserGamificationEvents.Add(gamificationEvent);

        _db.PointsLedgers.Add(new PointsLedger
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            UserGamificationEventId = gamificationEvent.Id,
            PointsDelta = rule.Points,
            BalanceAfter = decimal.Round(lastBalance + rule.Points, 2),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return true;
    }
}
