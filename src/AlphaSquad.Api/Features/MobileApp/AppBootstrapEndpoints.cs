namespace AlphaSquad.Api.Features.MobileApp;

/// <summary>
/// Entrega um bootstrap autenticado para o app mobile.
/// Esse endpoint reduz round-trips no primeiro carregamento e ajuda o cliente a montar shell, branding e atalhos iniciais.
/// </summary>
public static class AppBootstrapEndpoints
{
    public static IEndpointRouteBuilder MapAppBootstrapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/app/bootstrap", GetBootstrapAsync)
            .WithTags("App")
            .RequireAuthorization()
            .WithName("GetAppBootstrap")
            .WithSummary("Retorna o bootstrap autenticado do app mobile.")
            .WithDescription("Consolida sessao, tenant atual, features liberadas, contador de notificacoes e resumo leve de gamificacao para a home do app.")
            .Produces<AppBootstrapResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> GetBootstrapAsync(AppDbContext db, HttpContext context)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantIdClaim = context.User.FindFirstValue("tenant_id");
        var tenantSlug = context.User.FindFirstValue("tenant_slug");

        if (string.IsNullOrWhiteSpace(userIdClaim) || string.IsNullOrWhiteSpace(tenantIdClaim))
            return Results.Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var tenantId = Guid.Parse(tenantIdClaim);

        var authenticatedUser = await AuthEndpoints.BuildAuthenticatedUserResponseAsync(userId, tenantId, db);
        if (authenticatedUser is null)
            return Results.Unauthorized();

        var session = new AuthenticatedSessionResponse(
            authenticatedUser.Id,
            authenticatedUser.Name,
            authenticatedUser.Email,
            authenticatedUser.Role,
            authenticatedUser.Username,
            authenticatedUser.ProfilePhotoUrl,
            authenticatedUser.ActivePlanId,
            authenticatedUser.ActivePlan,
            authenticatedUser.ActivePlanPrice,
            authenticatedUser.ActivePlanDurationDays,
            authenticatedUser.MustChangePassword,
            tenantId,
            tenantSlug ?? string.Empty
        );

        var tenant = await GetCurrentTenantAsync(tenantId, db);
        if (tenant is null)
            return Results.NotFound();

        var features = await GetTenantFeaturesAsync(tenantId, db);
        var unreadNotificationsCount = await GetUnreadNotificationsCountAsync(tenantId, userId, session.Role, db);
        var gamification = session.Role == UserRole.Student
            ? await BuildGamificationSummaryAsync(tenantId, userId, db)
            : null;

        return Results.Ok(new AppBootstrapResponse(
            DateTime.UtcNow,
            session,
            tenant,
            features,
            unreadNotificationsCount,
            gamification
        ));
    }

    private static async Task<TenantCurrentResponse?> GetCurrentTenantAsync(Guid tenantId, AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT id,
                                    name,
                                    slug,
                                    logo_url AS LogoUrl,
                                    primary_color AS PrimaryColor,
                                    secondary_color AS SecondaryColor,
                                    is_active AS IsActive
                             FROM tenants
                             WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<TenantCurrentResponse>(sql, new { Id = tenantId });
    }

    private static async Task<TenantFeaturesResponse> GetTenantFeaturesAsync(Guid tenantId, AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT f.name,
                                    f.description
                             FROM tenant_features tf
                             JOIN features f ON tf.feature_id = f.id
                             WHERE tf.tenant_id = @TenantId
                             ORDER BY f.name ASC";

        var features = await connection.QueryAsync<TenantFeaturesResponse.FeatureItem>(sql, new { TenantId = tenantId });
        return new TenantFeaturesResponse(features.ToList());
    }

    private static async Task<int> GetUnreadNotificationsCountAsync(Guid tenantId, Guid userId, UserRole role, AppDbContext db)
    {
        var now = DateTime.UtcNow;
        var visible = ApplyNotificationVisibility(db.TenantNotifications, role)
            .Where(x =>
                x.TenantId == tenantId &&
                x.IsActive &&
                x.PublishedAt <= now &&
                (!x.ExpiresAt.HasValue || x.ExpiresAt > now));

        return await visible.CountAsync(x =>
            !db.UserNotificationReads.Any(read =>
                read.TenantNotificationId == x.Id &&
                read.TenantId == tenantId &&
                read.UserId == userId));
    }

    private static IQueryable<TenantNotification> ApplyNotificationVisibility(IQueryable<TenantNotification> query, UserRole role)
    {
        return role == UserRole.Student
            ? query.Where(x => x.Audience == TenantNotificationAudience.StudentsOnly || x.Audience == TenantNotificationAudience.AllTenantUsers)
            : query.Where(x => x.Audience == TenantNotificationAudience.AllTenantUsers);
    }

    private static async Task<AppBootstrapGamificationSummaryResponse> BuildGamificationSummaryAsync(Guid tenantId, Guid userId, AppDbContext db)
    {
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

        var summary = await connection.QueryFirstAsync<BootstrapGamificationSummaryProjection>(summarySql, new
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

        return new AppBootstrapGamificationSummaryResponse(
            now.Year,
            now.Month,
            summary.CurrentMonthPoints,
            currentMonthPosition,
            totalAccumulatedPoints,
            summary.CurrentMonthEventCount
        );
    }

    private sealed record BootstrapGamificationSummaryProjection(
        decimal CurrentMonthPoints,
        int CurrentMonthEventCount
    );
}
