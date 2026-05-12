using AlphaSquad.Shared.Enums;

namespace AlphaSquad.Api.Features.Checkins;

public static class CheckInEndpoints
{
    /// <summary>
    /// Registra os endpoints do mÃ³dulo de check-in.
    /// O grupo concentra o registro de entrada do usuÃ¡rio e as consultas por aluno e por tenant.
    /// </summary>
    public static IEndpointRouteBuilder MapCheckInEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/checkins")
            .WithTags("Checkins")
            .RequireAuthorization();

        group.MapPost("/", CreateAsync)
            .WithName("CreateCheckIn")
            .WithSummary("Registra um check-in para o usuÃ¡rio autenticado.")
            .WithDescription("Cria um registro de entrada no tenant atual, respeitando a regra de um check-in por dia.")
            .Produces<CheckInResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/me", GetMyCheckInsAsync)
            .WithName("GetMyCheckIns")
            .WithSummary("Lista os check-ins do usuÃ¡rio autenticado.")
            .WithDescription("Retorna o histÃ³rico paginado de check-ins do prÃ³prio usuÃ¡rio, com filtro opcional por perÃ­odo.")
            .Produces<PagedResponse<CheckInResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/tenant", GetTenantCheckInsAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetTenantCheckIns")
            .WithSummary("Lista os check-ins do tenant atual.")
            .WithDescription("Retorna os check-ins do tenant para visao administrativa, com filtro opcional por usuario e periodo.")
            .Produces<PagedResponse<CheckInResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/inactive-users", GetUsersWithoutRecentCheckInAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetUsersWithoutRecentCheckIn")
            .WithSummary("Lista usuarios ativos ha X dias sem check-in.")
            .WithDescription("Ajuda a academia a identificar alunos ativos com plano vigente que nao registram presenca ha um periodo minimo, apoiando campanhas de retorno e reengajamento.")
            .Produces<List<UserWithoutRecentCheckInResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Registra um novo check-in para o usuÃ¡rio autenticado.
    /// A regra atual permite apenas um check-in por dia para cada usuÃ¡rio.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateCheckInRequest? request,
                                                   AppDbContext db,
                                                   IGamificationService gamificationService,
                                                   HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        // Garante que o usuÃ¡rio ainda existe, estÃ¡ ativo e pertence ao tenant do token.
        var appUser = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (appUser is null)
            return Results.Unauthorized();

        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        // Evita mÃºltiplos check-ins do mesmo usuÃ¡rio no mesmo dia.
        var alreadyCheckedInToday = await db.CheckIns.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.UserId == userId &&
            x.CheckedInAt >= todayStart &&
            x.CheckedInAt < tomorrowStart);

        if (alreadyCheckedInToday)
            return Results.Conflict("User has already checked in today.");

        var checkIn = new CheckIn
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            CheckedInAt = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(request?.Notes) ? null : request.Notes.Trim()
        };

        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync();

        await gamificationService.AwardEventAsync(
            tenantId,
            userId,
            GamificationEventType.CheckIn,
            "checkin",
            checkIn.Id,
            checkIn.CheckedInAt,
            "Check-in processed successfully.");

        return Results.Created($"/api/checkins/{checkIn.Id}", new CheckInResponse(
            checkIn.Id,
            appUser.Id,
            appUser.Name,
            appUser.Role,
            checkIn.CheckedInAt,
            checkIn.Notes
        ));
    }

    /// <summary>
    /// Retorna o histÃ³rico de check-ins do usuÃ¡rio autenticado.
    /// Suporta filtro por perÃ­odo e paginaÃ§Ã£o simples.
    /// </summary>
    private static async Task<IResult> GetMyCheckInsAsync(AppDbContext db,
                                                          HttpContext context,
                                                          DateTime? dateFrom = null,
                                                          DateTime? dateTo = null,
                                                          int page = 1,
                                                          int pageSize = 20)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var connection = db.Database.GetDbConnection();
        var dateToExclusive = dateTo?.Date.AddDays(1);

        // A data final Ã© tratada como limite exclusivo para simplificar o filtro por dia.
        const string countSql = @"SELECT COUNT(*)
                                  FROM checkins c
                                  WHERE c.tenant_id = @TenantId
                                    AND c.user_id = @UserId
                                    AND (CAST(@DateFrom AS timestamptz) IS NULL OR c.checked_in_at >= CAST(@DateFrom AS timestamptz))
                                    AND (CAST(@DateToExclusive AS timestamptz) IS NULL OR c.checked_in_at < CAST(@DateToExclusive AS timestamptz))";

        const string itemsSql = @"SELECT c.id,
                                         c.user_id AS UserId,
                                         u.name AS UserName,
                                         u.role AS UserRole,
                                         c.checked_in_at AS CheckedInAt,
                                         c.notes
                                  FROM checkins c
                                  JOIN users u ON u.id = c.user_id
                                  WHERE c.tenant_id = @TenantId
                                    AND c.user_id = @UserId
                                    AND u.tenant_id = @TenantId
                                    AND (CAST(@DateFrom AS timestamptz) IS NULL OR c.checked_in_at >= CAST(@DateFrom AS timestamptz))
                                    AND (CAST(@DateToExclusive AS timestamptz) IS NULL OR c.checked_in_at < CAST(@DateToExclusive AS timestamptz))
                                  ORDER BY c.checked_in_at DESC
                                  LIMIT @Limit OFFSET @Offset";

        var parameters = new
        {
            TenantId = tenantId,
            UserId = userId,
            DateFrom = dateFrom,
            DateToExclusive = dateToExclusive,
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<CheckInResponse>(itemsSql, parameters);

        return Results.Ok(new PagedResponse<CheckInResponse>(page, pageSize, total, items.ToList()));
    }

    /// <summary>
    /// Lista usuarios ativos do tenant com plano vigente que estao ha X dias sem check-in.
    /// Inclui o ultimo check-in registrado para apoiar acoes de retencao e reengajamento.
    /// </summary>
    private static async Task<IResult> GetUsersWithoutRecentCheckInAsync(AppDbContext db, HttpContext context, int daysWithoutCheckIn = 7)
    {
        if (daysWithoutCheckIn < 1)
            return Results.BadRequest("daysWithoutCheckIn must be greater than zero.");

        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();
        var now = DateTime.UtcNow;
        var threshold = now.AddDays(-daysWithoutCheckIn);

        const string sql = @"SELECT u.id AS UserId,
                                    u.name AS UserName,
                                    u.email,
                                    u.role AS UserRole,
                                    last_checkin.last_checked_in_at AS LastCheckedInAt,
                                    CASE
                                        WHEN last_checkin.last_checked_in_at IS NULL THEN NULL
                                        ELSE CAST(EXTRACT(DAY FROM (@Now - last_checkin.last_checked_in_at)) AS INTEGER)
                                    END AS DaysWithoutCheckIn,
                                    active_plan.membership_plan_id AS ActivePlanId,
                                    active_plan.plan_name AS ActivePlanName
                             FROM users u
                             JOIN LATERAL (
                                 SELECT um.membership_plan_id,
                                        mp.name AS plan_name
                                 FROM user_memberships um
                                 JOIN membership_plans mp
                                   ON mp.id = um.membership_plan_id
                                  AND mp.tenant_id = um.tenant_id
                                 WHERE um.user_id = u.id
                                   AND um.tenant_id = u.tenant_id
                                   AND um.is_active = true
                                   AND mp.is_active = true
                                   AND um.starts_at <= @Now
                                   AND (um.ends_at IS NULL OR um.ends_at > @Now)
                                 ORDER BY um.starts_at DESC, um.created_at DESC
                                 LIMIT 1
                             ) active_plan ON true
                             LEFT JOIN LATERAL (
                                 SELECT MAX(c.checked_in_at) AS last_checked_in_at
                                 FROM checkins c
                                 WHERE c.user_id = u.id
                                   AND c.tenant_id = u.tenant_id
                             ) last_checkin ON true
                             WHERE u.tenant_id = @TenantId
                               AND u.is_active = true
                               AND (last_checkin.last_checked_in_at IS NULL OR last_checkin.last_checked_in_at <= @Threshold)
                             ORDER BY last_checkin.last_checked_in_at NULLS FIRST, u.name";

        var users = await connection.QueryAsync<UserWithoutRecentCheckInResponse>(sql, new
        {
            TenantId = tenantId,
            Threshold = threshold,
            Now = now
        });

        return Results.Ok(users.ToList());
    }

    /// <summary>
    /// Retorna os check-ins do tenant atual para visÃ£o administrativa.
    /// O acesso Ã© restrito aos perfis Admin e Teacher.
    /// </summary>
    private static async Task<IResult> GetTenantCheckInsAsync(AppDbContext db,
                                                              HttpContext context,
                                                              Guid? userId = null,
                                                              DateTime? dateFrom = null,
                                                              DateTime? dateTo = null,
                                                              int page = 1,
                                                              int pageSize = 20)
    {
        var tenantId = context.GetTenantId();
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        // Quando um usuÃ¡rio especÃ­fico Ã© informado, validamos se ele pertence ao tenant atual.
        if (userId.HasValue)
        {
            var userExists = await db.Users.AnyAsync(x => x.Id == userId.Value && x.TenantId == tenantId && x.IsActive);
            if (!userExists)
                return Results.BadRequest("User does not belong to this tenant.");
        }

        var connection = db.Database.GetDbConnection();
        var dateToExclusive = dateTo?.Date.AddDays(1);

        const string countSql = @"SELECT COUNT(*)
                                  FROM checkins c
                                  JOIN users u ON u.id = c.user_id
                                  WHERE c.tenant_id = @TenantId
                                    AND u.tenant_id = @TenantId
                                    AND (CAST(@UserId AS uuid) IS NULL OR c.user_id = CAST(@UserId AS uuid))
                                    AND (CAST(@DateFrom AS timestamptz) IS NULL OR c.checked_in_at >= CAST(@DateFrom AS timestamptz))
                                    AND (CAST(@DateToExclusive AS timestamptz) IS NULL OR c.checked_in_at < CAST(@DateToExclusive AS timestamptz))";

        const string itemsSql = @"SELECT c.id,
                                         c.user_id AS UserId,
                                         u.name AS UserName,
                                         u.role AS UserRole,
                                         c.checked_in_at AS CheckedInAt,
                                         c.notes
                                  FROM checkins c
                                  JOIN users u ON u.id = c.user_id
                                  WHERE c.tenant_id = @TenantId
                                    AND u.tenant_id = @TenantId
                                    AND (CAST(@UserId AS uuid) IS NULL OR c.user_id = CAST(@UserId AS uuid))
                                    AND (CAST(@DateFrom AS timestamptz) IS NULL OR c.checked_in_at >= CAST(@DateFrom AS timestamptz))
                                    AND (CAST(@DateToExclusive AS timestamptz) IS NULL OR c.checked_in_at < CAST(@DateToExclusive AS timestamptz))
                                  ORDER BY c.checked_in_at DESC
                                  LIMIT @Limit OFFSET @Offset";

        var parameters = new
        {
            TenantId = tenantId,
            UserId = userId,
            DateFrom = dateFrom,
            DateToExclusive = dateToExclusive,
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<CheckInResponse>(itemsSql, parameters);

        return Results.Ok(new PagedResponse<CheckInResponse>(page, pageSize, total, items.ToList()));
    }

    /// <summary>
    /// Extrai o identificador do usuÃ¡rio autenticado a partir das claims do JWT.
    /// </summary>
    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User not found in token.");

        return Guid.Parse(userIdClaim);
    }

}

