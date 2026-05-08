using AlphaSquad.Shared.Enums;

namespace AlphaSquad.Api.Features.Checkins;

public static class CheckInEndpoints
{
    /// <summary>
    /// Registra os endpoints do módulo de check-in.
    /// O grupo concentra o registro de entrada do usuário e as consultas por aluno e por tenant.
    /// </summary>
    public static IEndpointRouteBuilder MapCheckInEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/checkins")
            .WithTags("Checkins")
            .RequireAuthorization();

        group.MapPost("/", CreateAsync)
            .WithName("CreateCheckIn")
            .WithSummary("Registra um check-in para o usuário autenticado.")
            .WithDescription("Cria um registro de entrada no tenant atual, respeitando a regra de um check-in por dia.")
            .Produces<CheckInResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/me", GetMyCheckInsAsync)
            .WithName("GetMyCheckIns")
            .WithSummary("Lista os check-ins do usuário autenticado.")
            .WithDescription("Retorna o histórico paginado de check-ins do próprio usuário, com filtro opcional por período.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/tenant", GetTenantCheckInsAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("GetTenantCheckIns")
            .WithSummary("Lista os check-ins do tenant atual.")
            .WithDescription("Retorna os check-ins do tenant para visão administrativa, com filtro opcional por usuário e período.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Registra um novo check-in para o usuário autenticado.
    /// A regra atual permite apenas um check-in por dia para cada usuário.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateCheckInRequest? request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        // Garante que o usuário ainda existe, está ativo e pertence ao tenant do token.
        var appUser = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (appUser is null)
            return Results.Unauthorized();

        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        // Evita múltiplos check-ins do mesmo usuário no mesmo dia.
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
    /// Retorna o histórico de check-ins do usuário autenticado.
    /// Suporta filtro por período e paginação simples.
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

        // A data final é tratada como limite exclusivo para simplificar o filtro por dia.
        const string countSql = @"SELECT COUNT(*)
                                  FROM checkins c
                                  WHERE c.tenant_id = @TenantId
                                    AND c.user_id = @UserId
                                    AND (@DateFrom IS NULL OR c.checked_in_at >= @DateFrom)
                                    AND (@DateToExclusive IS NULL OR c.checked_in_at < @DateToExclusive)";

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
                                    AND (@DateFrom IS NULL OR c.checked_in_at >= @DateFrom)
                                    AND (@DateToExclusive IS NULL OR c.checked_in_at < @DateToExclusive)
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

        return Results.Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }

    /// <summary>
    /// Retorna os check-ins do tenant atual para visão administrativa.
    /// O acesso é restrito aos perfis Admin e Teacher.
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

        // Quando um usuário específico é informado, validamos se ele pertence ao tenant atual.
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
                                    AND (@UserId IS NULL OR c.user_id = @UserId)
                                    AND (@DateFrom IS NULL OR c.checked_in_at >= @DateFrom)
                                    AND (@DateToExclusive IS NULL OR c.checked_in_at < @DateToExclusive)";

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
                                    AND (@UserId IS NULL OR c.user_id = @UserId)
                                    AND (@DateFrom IS NULL OR c.checked_in_at >= @DateFrom)
                                    AND (@DateToExclusive IS NULL OR c.checked_in_at < @DateToExclusive)
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

        return Results.Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }

    /// <summary>
    /// Extrai o identificador do usuário autenticado a partir das claims do JWT.
    /// </summary>
    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User not found in token.");

        return Guid.Parse(userIdClaim);
    }

}
