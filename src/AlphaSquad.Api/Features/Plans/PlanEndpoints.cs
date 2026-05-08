namespace AlphaSquad.Api.Features.Plans;

/// <summary>
/// Registra os endpoints do dominio de planos.
/// Neste primeiro recorte, o modulo e administrativo e serve de base real para o campo ActivePlan do profile.
/// </summary>
public static class PlanEndpoints
{
    public static IEndpointRouteBuilder MapPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plans")
            .WithTags("Plans")
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher);

        group.MapGet("/", GetAllAsync)
            .WithName("GetPlans")
            .WithSummary("Lista os planos do tenant atual.")
            .WithDescription("Retorna o catalogo de planos cadastrados para a academia autenticada.")
            .Produces<List<MembershipPlanResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateAsync)
            .WithName("CreatePlan")
            .WithSummary("Cria um novo plano para o tenant atual.")
            .WithDescription("Cadastra um plano com nome, descricao, preco e duracao padrao em dias.")
            .Produces<MembershipPlanResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdatePlan")
            .WithSummary("Atualiza um plano do tenant atual.")
            .WithDescription("Permite alterar os dados administrativos e o status de um plano existente.")
            .Produces<MembershipPlanResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/{id:guid}/assign", AssignAsync)
            .WithName("AssignPlanToUser")
            .WithSummary("Atribui um plano a um usuario do tenant atual.")
            .WithDescription("Encerra o plano ativo anterior do usuario, quando existir, e registra o novo vinculo como plano ativo.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/users/{userId:guid}/history", GetUserHistoryAsync)
            .WithName("GetUserPlanHistory")
            .WithSummary("Retorna o historico de planos de um usuario.")
            .WithDescription("Lista os vinculos de planos do usuario, incluindo vigencia, motivo do status e quem realizou a alteracao administrativa.")
            .Produces<List<UserMembershipHistoryResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/inactive-users", GetUsersWithoutActivePlanAsync)
            .WithName("GetUsersWithoutActivePlan")
            .WithSummary("Lista usuarios sem plano ativo ha X dias.")
            .WithDescription("Ajuda a academia a identificar alunos sem plano ativo por um periodo minimo para futuras campanhas de reativacao.")
            .Produces<List<UserWithoutActivePlanResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Lista os planos do tenant autenticado.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT id,
                                    name,
                                    description,
                                    price,
                                    duration_days AS DurationDays,
                                    is_active AS IsActive,
                                    created_at AS CreatedAt
                             FROM membership_plans
                             WHERE tenant_id = @TenantId
                             ORDER BY name";

        var plans = await connection.QueryAsync<MembershipPlanResponse>(sql, new { TenantId = tenantId });
        return Results.Ok(plans.ToList());
    }

    /// <summary>
    /// Cria um novo plano no tenant atual.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateMembershipPlanRequest request, AppDbContext db, HttpContext context)
    {
        var validation = ValidatePlanRequest(request.Name, request.Price, request.DurationDays);
        if (validation is not null)
            return validation;

        var tenantId = context.GetTenantId();

        var plan = new MembershipPlan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            Price = decimal.Round(request.Price, 2),
            DurationDays = request.DurationDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.MembershipPlans.Add(plan);
        await db.SaveChangesAsync();

        return Results.Created($"/api/plans/{plan.Id}", MapResponse(plan));
    }

    /// <summary>
    /// Atualiza um plano do tenant atual.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id, UpdateMembershipPlanRequest request, AppDbContext db, HttpContext context)
    {
        var validation = ValidatePlanRequest(request.Name, request.Price, request.DurationDays);
        if (validation is not null)
            return validation;

        var tenantId = context.GetTenantId();
        var plan = await db.MembershipPlans.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (plan is null)
            return Results.NotFound();

        plan.Name = request.Name.Trim();
        plan.Description = NormalizeOptional(request.Description);
        plan.Price = decimal.Round(request.Price, 2);
        plan.DurationDays = request.DurationDays;
        plan.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return Results.Ok(MapResponse(plan));
    }

    /// <summary>
    /// Atribui um plano a um usuario do tenant atual e garante um unico plano ativo por usuario.
    /// </summary>
    private static async Task<IResult> AssignAsync(Guid id, AssignMembershipPlanRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var actorUserId = GetUserId(context.User);

        var plan = await db.MembershipPlans.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.IsActive);
        if (plan is null)
            return Results.NotFound();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == request.UserId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.BadRequest("User does not belong to this tenant.");

        if (request.EndsAt.HasValue && request.EndsAt.Value <= request.StartsAt)
            return Results.BadRequest("End date must be greater than start date.");

        var normalizedReason = NormalizeOptional(request.StatusReason);

        var currentMemberships = await db.UserMemberships
            .Where(x => x.UserId == request.UserId && x.TenantId == tenantId && x.IsActive)
            .ToListAsync();

        if (currentMemberships.Any(x => x.MembershipPlanId == id))
            return Results.BadRequest("User already has this plan as the active membership.");

        foreach (var membership in currentMemberships)
        {
            membership.IsActive = false;
            if (!membership.EndsAt.HasValue || membership.EndsAt > request.StartsAt)
                membership.EndsAt = request.StartsAt;
            membership.StatusReason = normalizedReason ?? $"Replaced by plan {plan.Name}.";
            membership.ChangedByUserId = actorUserId;
        }

        db.UserMemberships.Add(new UserMembership
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = request.UserId,
            MembershipPlanId = id,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            IsActive = true,
            StatusReason = normalizedReason,
            ChangedByUserId = actorUserId,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    /// <summary>
    /// Retorna o historico completo de planos do usuario informado.
    /// </summary>
    private static async Task<IResult> GetUserHistoryAsync(Guid userId, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var userExists = await db.Users.AnyAsync(x => x.Id == userId && x.TenantId == tenantId);
        if (!userExists)
            return Results.BadRequest("User does not belong to this tenant.");

        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT um.id,
                                    um.user_id AS UserId,
                                    u.name AS UserName,
                                    um.membership_plan_id AS MembershipPlanId,
                                    mp.name AS PlanName,
                                    mp.price AS PlanPrice,
                                    mp.duration_days AS DurationDays,
                                    um.starts_at AS StartsAt,
                                    um.ends_at AS EndsAt,
                                    um.is_active AS IsActive,
                                    um.status_reason AS StatusReason,
                                    um.changed_by_user_id AS ChangedByUserId,
                                    changed_by.name AS ChangedByUserName,
                                    um.created_at AS CreatedAt
                             FROM user_memberships um
                             JOIN users u
                               ON u.id = um.user_id
                              AND u.tenant_id = um.tenant_id
                             JOIN membership_plans mp
                               ON mp.id = um.membership_plan_id
                              AND mp.tenant_id = um.tenant_id
                             LEFT JOIN users changed_by
                               ON changed_by.id = um.changed_by_user_id
                              AND changed_by.tenant_id = um.tenant_id
                             WHERE um.tenant_id = @TenantId
                               AND um.user_id = @UserId
                             ORDER BY um.starts_at DESC, um.created_at DESC";

        var history = await connection.QueryAsync<UserMembershipHistoryResponse>(sql, new
        {
            TenantId = tenantId,
            UserId = userId
        });

        return Results.Ok(history.ToList());
    }

    /// <summary>
    /// Lista usuarios que estao sem plano ativo ha pelo menos X dias.
    /// </summary>
    private static async Task<IResult> GetUsersWithoutActivePlanAsync(AppDbContext db, HttpContext context, int daysWithoutPlan = 30)
    {
        if (daysWithoutPlan < 1)
            return Results.BadRequest("daysWithoutPlan must be greater than zero.");

        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();
        var now = DateTime.UtcNow;
        var threshold = now.AddDays(-daysWithoutPlan);

        const string sql = @"SELECT u.id AS UserId,
                                    u.name AS UserName,
                                    u.email,
                                    u.role,
                                    last_membership.membership_plan_id AS LastPlanId,
                                    last_membership.plan_name AS LastPlanName,
                                    last_membership.ends_at AS LastPlanEndedAt,
                                    CAST(EXTRACT(DAY FROM (@Now - last_membership.ends_at)) AS INTEGER) AS DaysWithoutActivePlan
                             FROM users u
                             LEFT JOIN LATERAL (
                                 SELECT um.membership_plan_id,
                                        mp.name AS plan_name,
                                        um.ends_at
                                 FROM user_memberships um
                                 JOIN membership_plans mp
                                   ON mp.id = um.membership_plan_id
                                  AND mp.tenant_id = um.tenant_id
                                 WHERE um.user_id = u.id
                                   AND um.tenant_id = u.tenant_id
                                 ORDER BY COALESCE(um.ends_at, um.starts_at) DESC, um.created_at DESC
                                 LIMIT 1
                             ) last_membership ON true
                             WHERE u.tenant_id = @TenantId
                               AND u.is_active = true
                               AND last_membership.ends_at IS NOT NULL
                               AND last_membership.ends_at <= @Threshold
                               AND NOT EXISTS (
                                   SELECT 1
                                   FROM user_memberships active_um
                                   JOIN membership_plans active_mp
                                     ON active_mp.id = active_um.membership_plan_id
                                    AND active_mp.tenant_id = active_um.tenant_id
                                   WHERE active_um.user_id = u.id
                                     AND active_um.tenant_id = u.tenant_id
                                     AND active_um.is_active = true
                                     AND active_mp.is_active = true
                                     AND active_um.starts_at <= @Now
                                     AND (active_um.ends_at IS NULL OR active_um.ends_at > @Now)
                               )
                             ORDER BY last_membership.ends_at ASC, u.name";

        var users = await connection.QueryAsync<UserWithoutActivePlanResponse>(sql, new
        {
            TenantId = tenantId,
            Threshold = threshold,
            Now = now
        });

        return Results.Ok(users.ToList());
    }

    /// <summary>
    /// Valida os dados basicos de um plano antes da persistencia.
    /// </summary>
    private static IResult? ValidatePlanRequest(string name, decimal price, int durationDays)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Plan name is required.");

        if (price < 0)
            return Results.BadRequest("Plan price cannot be negative.");

        if (durationDays <= 0)
            return Results.BadRequest("Plan duration must be greater than zero.");

        return null;
    }

    /// <summary>
    /// Converte a entidade para o contrato de resposta da API.
    /// </summary>
    private static MembershipPlanResponse MapResponse(MembershipPlan plan)
    {
        return new MembershipPlanResponse(
            plan.Id,
            plan.Name,
            plan.Description,
            plan.Price,
            plan.DurationDays,
            plan.IsActive,
            plan.CreatedAt
        );
    }

    /// <summary>
    /// Normaliza campos opcionais para evitar persistencia de textos vazios.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
}
