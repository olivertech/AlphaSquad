namespace AlphaSquad.Api.Features.Gamification;

/// <summary>
/// Registra os endpoints do modulo de gamificacao.
/// Esta primeira rodada entrega regras, dashboard pessoal, ranking mensal e historico de vencedores.
/// </summary>
public static class GamificationEndpoints
{
    public static IEndpointRouteBuilder MapGamificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/gamification")
            .WithTags("Gamification")
            .RequireAuthorization();

        group.MapGet("/me", GetMyDashboardAsync)
            .WithName("GetMyGamificationDashboard")
            .WithSummary("Retorna o dashboard de gamificacao do aluno autenticado.")
            .WithDescription("Consolida pontuacao do mes atual, posicao no ranking, saldo acumulado e eventos recentes do proprio aluno.")
            .Produces<MyGamificationDashboardResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<GamificationAccessMessageResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/ranking/monthly", GetMonthlyRankingAsync)
            .WithName("GetMonthlyRanking")
            .WithSummary("Retorna o ranking mensal de alunos do tenant.")
            .WithDescription("Lista a classificacao do mes por pontuacao, considerando apenas alunos e eventos de gamificacao do tenant.")
            .Produces<MonthlyRankingResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/winners/history", GetWinnersHistoryAsync)
            .WithName("GetGamificationWinnersHistory")
            .WithSummary("Retorna o historico de vencedores mensais da gamificacao.")
            .WithDescription("Lista os snapshots fechados de ranking para consulta dos vencedores e premios de meses anteriores.")
            .Produces<List<MonthlyWinnerHistoryEntryResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/rules", GetRulesAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetGamificationRules")
            .WithSummary("Lista as regras de pontuacao do tenant.")
            .WithDescription("Retorna as regras administrativas que definem quantos pontos cada evento vale no tenant atual.")
            .Produces<List<GamificationEventRuleResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/rules", CreateRuleAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateGamificationRule")
            .WithSummary("Cria uma nova regra de pontuacao.")
            .WithDescription("Permite cadastrar uma regra administrativa para definir a pontuacao de um tipo de evento.")
            .Produces<GamificationEventRuleResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/rules/{id:guid}", UpdateRuleAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateGamificationRule")
            .WithSummary("Atualiza uma regra de pontuacao.")
            .WithDescription("Permite ajustar nome, descricao, pontos e status ativo de uma regra existente do tenant.")
            .Produces<GamificationEventRuleResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/ranking/monthly/close", CloseMonthlyRankingAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CloseMonthlyRanking")
            .WithSummary("Fecha o ranking mensal da gamificacao.")
            .WithDescription("Gera ou substitui o snapshot do ranking mensal para preservar vencedores, posicoes e premios do periodo.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Retorna o dashboard de gamificacao do proprio aluno.
    /// Quando o usuario autenticado nao e aluno, a API responde com uma mensagem clara informando que ele nao participa da gamificacao.
    /// </summary>
    private static async Task<IResult> GetMyDashboardAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        if (user.Role != UserRole.Student)
        {
            return Results.Json(
                new GamificationAccessMessageResponse("This user does not participate in gamification. Only students can access student gamification data."),
                statusCode: StatusCodes.Status403Forbidden);
        }

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        var connection = db.Database.GetDbConnection();

        const string summarySql = @"SELECT COALESCE(SUM(e.points_applied), 0) AS CurrentMonthPoints,
                                           COUNT(*) AS CurrentMonthEventCount
                                    FROM user_gamification_events e
                                    WHERE e.tenant_id = @TenantId
                                      AND e.user_id = @UserId
                                      AND e.occurred_at >= @MonthStart
                                      AND e.occurred_at < @NextMonthStart";

        const string positionSql = @"SELECT ranking.position
                                     FROM (
                                         SELECT e.user_id,
                                                SUM(e.points_applied) AS total_points,
                                                ROW_NUMBER() OVER (ORDER BY SUM(e.points_applied) DESC, MIN(e.occurred_at) ASC, e.user_id ASC) AS position
                                         FROM user_gamification_events e
                                         JOIN users u
                                           ON u.id = e.user_id
                                          AND u.tenant_id = e.tenant_id
                                         WHERE e.tenant_id = @TenantId
                                           AND e.occurred_at >= @MonthStart
                                           AND e.occurred_at < @NextMonthStart
                                           AND u.role = @StudentRole
                                         GROUP BY e.user_id
                                     ) ranking
                                     WHERE ranking.user_id = @UserId";

        const string totalBalanceSql = @"SELECT COALESCE((
                                             SELECT balance_after
                                             FROM points_ledger
                                             WHERE tenant_id = @TenantId
                                               AND user_id = @UserId
                                             ORDER BY created_at DESC, id DESC
                                             LIMIT 1
                                         ), 0)";

        const string recentEventsSql = @"SELECT e.id,
                                                e.event_type AS EventType,
                                                r.name AS RuleName,
                                                e.points_applied AS PointsApplied,
                                                e.source_entity AS SourceEntity,
                                                e.source_entity_id AS SourceEntityId,
                                                e.occurred_at AS OccurredAt,
                                                e.notes
                                         FROM user_gamification_events e
                                         JOIN gamification_event_rules r
                                           ON r.id = e.gamification_event_rule_id
                                          AND r.tenant_id = e.tenant_id
                                         WHERE e.tenant_id = @TenantId
                                           AND e.user_id = @UserId
                                         ORDER BY e.occurred_at DESC, e.created_at DESC
                                         LIMIT 10";

        var summary = await connection.QueryFirstAsync<MyDashboardSummaryProjection>(summarySql, new
        {
            TenantId = tenantId,
            UserId = userId,
            MonthStart = monthStart,
            NextMonthStart = nextMonthStart
        });

        var currentMonthPosition = await connection.ExecuteScalarAsync<int?>(positionSql, new
        {
            TenantId = tenantId,
            UserId = userId,
            MonthStart = monthStart,
            NextMonthStart = nextMonthStart,
            StudentRole = UserRole.Student
        });

        var totalAccumulatedPoints = await connection.ExecuteScalarAsync<decimal>(totalBalanceSql, new
        {
            TenantId = tenantId,
            UserId = userId
        });

        var recentEvents = await connection.QueryAsync<UserGamificationEventResponse>(recentEventsSql, new
        {
            TenantId = tenantId,
            UserId = userId
        });

        return Results.Ok(new MyGamificationDashboardResponse(
            user.Id,
            user.Name,
            monthStart.Year,
            monthStart.Month,
            summary.CurrentMonthPoints,
            currentMonthPosition,
            totalAccumulatedPoints,
            summary.CurrentMonthEventCount,
            recentEvents.ToList()
        ));
    }

    /// <summary>
    /// Retorna o ranking mensal de alunos do tenant.
    /// Se existir snapshot fechado para o periodo, ele e priorizado para manter consistencia historica.
    /// </summary>
    private static async Task<IResult> GetMonthlyRankingAsync(AppDbContext db, HttpContext context, int? year = null, int? month = null, int top = 50)
    {
        var tenantId = context.GetTenantId();
        var now = DateTime.UtcNow;
        var effectiveYear = year ?? now.Year;
        var effectiveMonth = month ?? now.Month;

        if (effectiveMonth is < 1 or > 12)
            return Results.BadRequest("Month must be between 1 and 12.");

        top = top is < 1 or > 100 ? 50 : top;

        var connection = db.Database.GetDbConnection();

        const string snapshotSql = @"SELECT r.user_id AS UserId,
                                            u.name AS UserName,
                                            r.total_points AS TotalPoints,
                                            r.position,
                                            r.prize_description AS PrizeDescription
                                     FROM monthly_student_rankings r
                                     JOIN users u
                                       ON u.id = r.user_id
                                      AND u.tenant_id = r.tenant_id
                                     WHERE r.tenant_id = @TenantId
                                       AND r.year = @Year
                                       AND r.month = @Month
                                     ORDER BY r.position ASC
                                     LIMIT @Top";

        var snapshotItems = (await connection.QueryAsync<MonthlyRankingEntryResponse>(snapshotSql, new
        {
            TenantId = tenantId,
            Year = effectiveYear,
            Month = effectiveMonth,
            Top = top
        })).ToList();

        if (snapshotItems.Count > 0)
            return Results.Ok(new MonthlyRankingResponse(effectiveYear, effectiveMonth, true, snapshotItems));

        var monthStart = new DateTime(effectiveYear, effectiveMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        const string liveSql = @"SELECT ranking.user_id AS UserId,
                                        ranking.user_name AS UserName,
                                        ranking.total_points AS TotalPoints,
                                        ranking.position,
                                        NULL AS PrizeDescription
                                 FROM (
                                     SELECT e.user_id,
                                            u.name AS user_name,
                                            SUM(e.points_applied) AS total_points,
                                            ROW_NUMBER() OVER (ORDER BY SUM(e.points_applied) DESC, MIN(e.occurred_at) ASC, e.user_id ASC) AS position
                                     FROM user_gamification_events e
                                     JOIN users u
                                       ON u.id = e.user_id
                                      AND u.tenant_id = e.tenant_id
                                     WHERE e.tenant_id = @TenantId
                                       AND e.occurred_at >= @MonthStart
                                       AND e.occurred_at < @NextMonthStart
                                       AND u.role = @StudentRole
                                     GROUP BY e.user_id, u.name
                                 ) ranking
                                 ORDER BY ranking.position ASC
                                 LIMIT @Top";

        var liveItems = (await connection.QueryAsync<MonthlyRankingEntryResponse>(liveSql, new
        {
            TenantId = tenantId,
            MonthStart = monthStart,
            NextMonthStart = nextMonthStart,
            StudentRole = UserRole.Student,
            Top = top
        })).ToList();

        return Results.Ok(new MonthlyRankingResponse(effectiveYear, effectiveMonth, false, liveItems));
    }

    /// <summary>
    /// Retorna o historico dos vencedores fechados em meses anteriores.
    /// </summary>
    private static async Task<IResult> GetWinnersHistoryAsync(AppDbContext db, HttpContext context, int limitMonths = 12)
    {
        var tenantId = context.GetTenantId();
        limitMonths = limitMonths is < 1 or > 36 ? 12 : limitMonths;

        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT r.year,
                                    r.month,
                                    r.position,
                                    r.user_id AS UserId,
                                    u.name AS UserName,
                                    r.total_points AS TotalPoints,
                                    r.prize_description AS PrizeDescription,
                                    r.generated_at AS GeneratedAt
                             FROM monthly_student_rankings r
                             JOIN users u
                               ON u.id = r.user_id
                              AND u.tenant_id = r.tenant_id
                             WHERE r.tenant_id = @TenantId
                               AND r.position <= 3
                             ORDER BY r.year DESC, r.month DESC, r.position ASC
                             LIMIT @Limit";

        var items = await connection.QueryAsync<MonthlyWinnerHistoryEntryResponse>(sql, new
        {
            TenantId = tenantId,
            Limit = limitMonths * 3
        });

        return Results.Ok(items.ToList());
    }

    /// <summary>
    /// Lista as regras de pontuacao do tenant atual.
    /// </summary>
    private static async Task<IResult> GetRulesAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT id,
                                    event_type AS EventType,
                                    name,
                                    description,
                                    points,
                                    is_active AS IsActive,
                                    created_at AS CreatedAt
                             FROM gamification_event_rules
                             WHERE tenant_id = @TenantId
                             ORDER BY event_type";

        var rules = await connection.QueryAsync<GamificationEventRuleResponse>(sql, new { TenantId = tenantId });
        return Results.Ok(rules.ToList());
    }

    /// <summary>
    /// Cria uma nova regra de pontuacao para o tenant atual.
    /// </summary>
    private static async Task<IResult> CreateRuleAsync(CreateGamificationEventRuleRequest request, AppDbContext db, HttpContext context)
    {
        var validation = ValidateRuleRequest(request.Name, request.Points);
        if (validation is not null)
            return validation;

        var tenantId = context.GetTenantId();
        var alreadyExists = await db.GamificationEventRules.AnyAsync(x => x.TenantId == tenantId && x.EventType == request.EventType);
        if (alreadyExists)
            return Results.BadRequest("A rule for this event type already exists in the tenant.");

        var rule = new GamificationEventRule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventType = request.EventType,
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            Points = decimal.Round(request.Points, 2),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.GamificationEventRules.Add(rule);
        await db.SaveChangesAsync();

        return Results.Created($"/api/gamification/rules/{rule.Id}", MapRule(rule));
    }

    /// <summary>
    /// Atualiza uma regra de pontuacao existente.
    /// </summary>
    private static async Task<IResult> UpdateRuleAsync(Guid id, UpdateGamificationEventRuleRequest request, AppDbContext db, HttpContext context)
    {
        var validation = ValidateRuleRequest(request.Name, request.Points);
        if (validation is not null)
            return validation;

        var tenantId = context.GetTenantId();
        var rule = await db.GamificationEventRules.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (rule is null)
            return Results.NotFound();

        rule.Name = request.Name.Trim();
        rule.Description = NormalizeOptional(request.Description);
        rule.Points = decimal.Round(request.Points, 2);
        rule.IsActive = request.IsActive;

        await db.SaveChangesAsync();
        return Results.Ok(MapRule(rule));
    }

    /// <summary>
    /// Fecha o ranking mensal com base nos eventos do periodo e registra os premios dos 3 primeiros.
    /// </summary>
    private static async Task<IResult> CloseMonthlyRankingAsync(CloseMonthlyRankingRequest request, AppDbContext db, HttpContext context)
    {
        if (request.Year < 2020)
            return Results.BadRequest("Year is invalid.");

        if (request.Month is < 1 or > 12)
            return Results.BadRequest("Month must be between 1 and 12.");

        var tenantId = context.GetTenantId();
        var monthStart = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);
        var connection = db.Database.GetDbConnection();

        // O fechamento e permitido apenas para meses ja iniciados, evitando snapshots vazios de periodos futuros.
        if (monthStart > currentMonthStart)
            return Results.BadRequest("Cannot close ranking for a future month.");

        const string sql = @"SELECT e.user_id AS UserId,
                                    SUM(e.points_applied) AS TotalPoints,
                                    ROW_NUMBER() OVER (ORDER BY SUM(e.points_applied) DESC, MIN(e.occurred_at) ASC, e.user_id ASC) AS Position
                             FROM user_gamification_events e
                             JOIN users u
                               ON u.id = e.user_id
                              AND u.tenant_id = e.tenant_id
                             WHERE e.tenant_id = @TenantId
                               AND e.occurred_at >= @MonthStart
                               AND e.occurred_at < @NextMonthStart
                               AND u.role = @StudentRole
                             GROUP BY e.user_id";

        var rankingRows = (await connection.QueryAsync<MonthlyRankingAggregationProjection>(sql, new
        {
            TenantId = tenantId,
            MonthStart = monthStart,
            NextMonthStart = nextMonthStart,
            StudentRole = UserRole.Student
        })).ToList();

        var existing = await db.MonthlyStudentRankings
            .Where(x => x.TenantId == tenantId && x.Year == request.Year && x.Month == request.Month)
            .ToListAsync();

        if (existing.Count > 0)
            db.MonthlyStudentRankings.RemoveRange(existing);

        foreach (var row in rankingRows)
        {
            db.MonthlyStudentRankings.Add(new MonthlyStudentRanking
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = row.UserId,
                Year = request.Year,
                Month = request.Month,
                Position = row.Position,
                TotalPoints = row.TotalPoints,
                PrizeDescription = row.Position switch
                {
                    1 => NormalizeOptional(request.FirstPlacePrize),
                    2 => NormalizeOptional(request.SecondPlacePrize),
                    3 => NormalizeOptional(request.ThirdPlacePrize),
                    _ => null
                },
                GeneratedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    /// <summary>
    /// Valida os dados basicos de uma regra de pontuacao.
    /// </summary>
    private static IResult? ValidateRuleRequest(string name, decimal points)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Rule name is required.");

        if (points <= 0)
            return Results.BadRequest("Rule points must be greater than zero.");

        return null;
    }

    /// <summary>
    /// Converte a entidade de regra para o contrato de resposta da API.
    /// </summary>
    private static GamificationEventRuleResponse MapRule(GamificationEventRule rule)
    {
        return new GamificationEventRuleResponse(
            rule.Id,
            rule.EventType,
            rule.Name,
            rule.Description,
            rule.Points,
            rule.IsActive,
            rule.CreatedAt
        );
    }

    /// <summary>
    /// Extrai o identificador do usuario autenticado a partir das claims do JWT.
    /// </summary>
    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User not found in token.");

        return Guid.Parse(userIdClaim);
    }

    /// <summary>
    /// Normaliza campos opcionais para evitar persistencia de espacos em branco.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Projecao interna do resumo do dashboard pessoal.
    /// </summary>
    private sealed record MyDashboardSummaryProjection(
        decimal CurrentMonthPoints,
        int CurrentMonthEventCount
    );

    /// <summary>
    /// Projecao interna usada no fechamento mensal do ranking.
    /// </summary>
    private sealed record MonthlyRankingAggregationProjection(
        Guid UserId,
        decimal TotalPoints,
        int Position
    );
}
