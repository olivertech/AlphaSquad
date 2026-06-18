namespace AlphaSquad.Api.Features.MobileApp;

using AlphaSquad.Infrastructure.Features;

/// <summary>
/// Entrega a home agregada do app mobile com seções prontas para a primeira dobra.
/// O objetivo é reduzir a orquestração no cliente e reaproveitar os contratos mobile já padronizados.
/// </summary>
public static class AppHomeEndpoints
{
    public static IEndpointRouteBuilder MapAppHomeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/app/home", GetHomeAsync)
            .WithTags("App")
            .RequireAuthorization()
            .WithName("GetAppHome")
            .WithSummary("Retorna a home agregada do app mobile.")
            .WithDescription("Consolida aulas próximas, destaques do mural, posts recentes e catálogo resumido da loja para a home do app.")
            .Produces<AppHomeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> GetHomeAsync(AppDbContext db,
                                                    IFeatureAccessService featureAccessService,
                                                    HttpContext context,
                                                    int sectionLimit = 5)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            return Results.Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var tenantId = context.GetTenantId();
        sectionLimit = sectionLimit is < 1 or > 10 ? 5 : sectionLimit;

        var upcomingClasses = await GetUpcomingClassesAsync(tenantId, userId, sectionLimit, db);
        var events = await GetEventSectionAsync(tenantId, userId, sectionLimit, featureAccessService, db);
        var social = await GetSocialSectionAsync(tenantId, userId, sectionLimit, featureAccessService, db);
        var store = await GetStoreSectionAsync(tenantId, sectionLimit, featureAccessService, db);

        return Results.Ok(new AppHomeResponse(
            DateTime.UtcNow,
            upcomingClasses,
            events,
            social,
            store
        ));
    }

    private static async Task<List<AppHomeClassSummaryResponse>> GetUpcomingClassesAsync(Guid tenantId,
                                                                                          Guid userId,
                                                                                          int limit,
                                                                                          AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        var now = DateTime.UtcNow;

        const string sql = @"SELECT gc.id,
                                    gc.name,
                                    gc.description,
                                    instructor.name AS InstructorName,
                                    gc.starts_at AS StartsAt,
                                    gc.ends_at AS EndsAt,
                                    gc.location,
                                    gc.is_special_class AS IsSpecialClass,
                                    CASE WHEN current_booking.id IS NULL THEN false ELSE true END AS IsUserBooked,
                                    gc.capacity,
                                    COALESCE(bookings.booked_count, 0) AS BookedCount
                             FROM gym_classes gc
                             LEFT JOIN users instructor
                               ON instructor.id = gc.instructor_user_id
                              AND instructor.tenant_id = gc.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS booked_count
                                 FROM class_bookings cb
                                 WHERE cb.gym_class_id = gc.id
                                   AND cb.tenant_id = gc.tenant_id
                             ) bookings ON true
                             LEFT JOIN LATERAL (
                                 SELECT cb.id
                                 FROM class_bookings cb
                                 WHERE cb.gym_class_id = gc.id
                                   AND cb.tenant_id = gc.tenant_id
                                   AND cb.user_id = @UserId
                                 LIMIT 1
                             ) current_booking ON true
                             WHERE gc.tenant_id = @TenantId
                               AND gc.is_active = true
                               AND gc.starts_at >= @Now
                             ORDER BY CASE WHEN current_booking.id IS NULL THEN 1 ELSE 0 END ASC,
                                      gc.starts_at ASC,
                                      gc.id ASC
                             LIMIT @Limit";

        return (await connection.QueryAsync<AppHomeClassSummaryResponse>(sql, new
        {
            TenantId = tenantId,
            UserId = userId,
            Now = now,
            Limit = limit
        })).ToList();
    }

    private static async Task<AppHomeCardSectionResponse> GetEventSectionAsync(Guid tenantId,
                                                                                Guid userId,
                                                                                int limit,
                                                                                IFeatureAccessService featureAccessService,
                                                                                AppDbContext db)
    {
        var enabled = await featureAccessService.HasFeatureAsync(tenantId, FeatureCodes.Events);
        if (!enabled)
            return new AppHomeCardSectionResponse("events", "Eventos em destaque", false, false, []);

        var connection = db.Database.GetDbConnection();
        var now = DateTime.UtcNow;

        const string sql = @"SELECT e.id,
                                    e.title,
                                    e.description,
                                    e.event_type AS EventType,
                                    e.media_id AS MediaId,
                                    m.url AS MediaUrl,
                                    e.location,
                                    e.starts_at AS StartsAt,
                                    e.ends_at AS EndsAt,
                                    e.is_highlighted AS IsHighlighted,
                                    e.highlight_starts_at AS HighlightStartsAt,
                                    e.highlight_ends_at AS HighlightEndsAt,
                                    e.is_outdoor_event AS IsOutdoorEvent,
                                    e.allow_participation AS AllowParticipation,
                                    e.is_active AS IsActive,
                                    NULL AS CheckInPassword,
                                    e.is_completed AS IsCompleted,
                                    e.created_by_user_id AS CreatedByUserId,
                                    creator.name AS CreatedByUserName,
                                    COALESCE(participants.participant_count, 0) AS ParticipantCount,
                                    CASE WHEN current_participation.id IS NULL THEN false ELSE true END AS IsUserParticipating,
                                    e.created_at AS CreatedAt,
                                    e.updated_at AS UpdatedAt
                             FROM academy_events e
                             LEFT JOIN tenant_medias m
                               ON m.id = e.media_id
                              AND m.tenant_id = e.tenant_id
                             JOIN users creator
                               ON creator.id = e.created_by_user_id
                              AND creator.tenant_id = e.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS participant_count
                                 FROM academy_event_participations ep
                                 WHERE ep.academy_event_id = e.id
                                   AND ep.tenant_id = e.tenant_id
                             ) participants ON true
                             LEFT JOIN LATERAL (
                                 SELECT ep.id
                                 FROM academy_event_participations ep
                                 WHERE ep.academy_event_id = e.id
                                   AND ep.tenant_id = e.tenant_id
                                   AND ep.user_id = @CurrentUserId
                                 LIMIT 1
                             ) current_participation ON true
                             WHERE e.tenant_id = @TenantId
                               AND e.is_active = true
                               AND (
                                   e.event_type = @StandardEventType
                                   OR (
                                       COALESCE(e.highlight_starts_at, e.created_at) <= @Now
                                       AND (e.highlight_ends_at IS NULL OR e.highlight_ends_at > @Now)
                                   )
                               )
                             ORDER BY e.is_highlighted DESC,
                                      COALESCE(e.highlight_starts_at, e.starts_at, e.created_at) DESC,
                                      e.id DESC
                             LIMIT @LimitPlusOne";

        var rows = (await connection.QueryAsync<AcademyEventResponse>(sql, new
        {
            TenantId = tenantId,
            CurrentUserId = userId,
            StandardEventType = AcademyEventType.Standard,
            Now = now,
            LimitPlusOne = limit + 1
        })).ToList();

        var hasMore = rows.Count > limit;
        var items = hasMore ? rows.Take(limit).ToList() : rows;
        return new AppHomeCardSectionResponse("events", "Eventos em destaque", true, hasMore, items.Select(x => x.MobileCard).ToList());
    }

    private static async Task<AppHomeCardSectionResponse> GetSocialSectionAsync(Guid tenantId,
                                                                                 Guid userId,
                                                                                 int limit,
                                                                                 IFeatureAccessService featureAccessService,
                                                                                 AppDbContext db)
    {
        var enabled = await featureAccessService.HasFeatureAsync(tenantId, FeatureCodes.Social);
        if (!enabled)
            return new AppHomeCardSectionResponse("social/posts", "Comunidade", false, false, []);

        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT p.id,
                                    p.user_id AS UserId,
                                    u.name AS UserName,
                                    up.username,
                                    up.profile_photo_url AS ProfilePhotoUrl,
                                    p.description,
                                    p.media_id AS MediaId,
                                    m.url AS MediaUrl,
                                    COALESCE(l.likes_count, 0) AS LikesCount,
                                    COALESCE(c.comments_count, 0) AS CommentsCount,
                                    CASE WHEN liked.id IS NULL THEN false ELSE true END AS IsLikedByCurrentUser,
                                    p.created_at AS CreatedAt,
                                    p.updated_at AS UpdatedAt
                             FROM social_posts p
                             JOIN users u
                               ON u.id = p.user_id
                              AND u.tenant_id = p.tenant_id
                             LEFT JOIN user_profiles up
                               ON up.user_id = p.user_id
                              AND up.tenant_id = p.tenant_id
                             LEFT JOIN tenant_medias m
                               ON m.id = p.media_id
                              AND m.tenant_id = p.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS likes_count
                                 FROM social_post_likes spl
                                 WHERE spl.social_post_id = p.id
                                   AND spl.tenant_id = p.tenant_id
                             ) l ON true
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS comments_count
                                 FROM social_post_comments spc
                                 WHERE spc.social_post_id = p.id
                                   AND spc.tenant_id = p.tenant_id
                             ) c ON true
                             LEFT JOIN LATERAL (
                                 SELECT spl.id
                                 FROM social_post_likes spl
                                 WHERE spl.social_post_id = p.id
                                   AND spl.tenant_id = p.tenant_id
                                   AND spl.user_id = @CurrentUserId
                                 LIMIT 1
                             ) liked ON true
                             WHERE p.tenant_id = @TenantId
                               AND p.is_active = true
                             ORDER BY p.created_at DESC, p.id DESC
                             LIMIT @LimitPlusOne";

        var rows = (await connection.QueryAsync<SocialPostResponse>(sql, new
        {
            TenantId = tenantId,
            CurrentUserId = userId,
            LimitPlusOne = limit + 1
        })).ToList();

        var hasMore = rows.Count > limit;
        var items = hasMore ? rows.Take(limit).ToList() : rows;
        return new AppHomeCardSectionResponse("social/posts", "Comunidade", true, hasMore, items.Select(x => x.MobileCard).ToList());
    }

    private static async Task<AppHomeCardSectionResponse> GetStoreSectionAsync(Guid tenantId,
                                                                                int limit,
                                                                                IFeatureAccessService featureAccessService,
                                                                                AppDbContext db)
    {
        var enabled = await featureAccessService.HasFeatureAsync(tenantId, FeatureCodes.Store);
        if (!enabled)
            return new AppHomeCardSectionResponse("store/products", "Loja da academia", false, false, []);

        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT p.id,
                                    p.name,
                                    p.description,
                                    m.url AS MainMediaUrl,
                                    (
                                        SELECT MIN(v.price)
                                        FROM product_variants v
                                        WHERE v.product_id = p.id
                                          AND v.tenant_id = p.tenant_id
                                          AND v.is_active = true
                                    ) AS StartingPrice,
                                    p.is_active AS IsActive,
                                    p.display_order AS DisplayOrder,
                                    p.created_at AS CreatedAt
                             FROM products p
                             LEFT JOIN tenant_medias m
                               ON m.id = p.main_media_id
                              AND m.tenant_id = p.tenant_id
                             WHERE p.tenant_id = @TenantId
                               AND p.is_active = true
                             ORDER BY p.display_order ASC, p.created_at DESC, p.id DESC
                             LIMIT @LimitPlusOne";

        var rows = (await connection.QueryAsync<ProductListItemResponse>(sql, new
        {
            TenantId = tenantId,
            LimitPlusOne = limit + 1
        })).ToList();

        var hasMore = rows.Count > limit;
        var items = hasMore ? rows.Take(limit).ToList() : rows;
        return new AppHomeCardSectionResponse("store/products", "Loja da academia", true, hasMore, items.Select(x => x.MobileCard).ToList());
    }
}
