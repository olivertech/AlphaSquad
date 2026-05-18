namespace AlphaSquad.Api.Features.Users;

using System.Globalization;
using System.Text.RegularExpressions;

public static class UserEndpoints
{
    /// <summary>
    /// Registra os endpoints administrativos de usuários do tenant.
    /// A leitura e a escrita ficam restritas a administradores por lidarem com cadastro, status e perfis de acesso.
    /// </summary>
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetUsers")
            .WithSummary("Lista os usuários do tenant atual.")
            .WithDescription("Retorna os usuários vinculados ao tenant da sessão, incluindo perfis ativos e inativos para uso administrativo.")
            .Produces<List<UserResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetUserById")
            .WithSummary("Busca um usuário específico do tenant atual.")
            .WithDescription("Retorna os dados de um usuário pelo identificador, respeitando o isolamento multi-tenant.")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateUser")
            .WithSummary("Cria um novo usuário no tenant atual.")
            .WithDescription("Cadastra um usuário com senha, role e vínculo ao tenant autenticado.")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateUser")
            .WithSummary("Atualiza um usuário do tenant atual.")
            .WithDescription("Permite alterar nome, role e status ativo de um usuário pertencente ao tenant da sessão.")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteUser")
            .WithSummary("Desativa um usuário do tenant atual.")
            .WithDescription("Realiza a exclusão lógica do usuário, marcando-o como inativo no banco.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/gamification-history", GetGamificationHistoryAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetUserGamificationHistory")
            .WithSummary("Retorna o histórico de pontuação do usuário.")
            .WithDescription("Lista os eventos individuais que geraram pontos para o usuário dentro da gamificação do tenant atual.")
            .Produces<UserGamificationHistoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Lista todos os usuários do tenant autenticado.
    /// A gestão precisa enxergar perfis ativos e inativos para conseguir auditar a base e reativar acessos quando necessário.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();
        var todayUtc = DateTime.UtcNow;
        var todayDate = todayUtc.Date;

        const string sql = @"SELECT u.id,
                                    u.tenant_id AS TenantId,
                                    u.name,
                                    u.email,
                                    u.role,
                                    u.is_active AS IsActive,
                                    u.created_at AS CreatedAt,
                                    up.phone_number AS PhoneNumber,
                                    CASE
                                        WHEN up.birth_date IS NULL THEN NULL
                                        ELSE TO_CHAR(up.birth_date, 'YYYY-MM-DD')
                                    END AS BirthDate,
                                    COALESCE(balance.balance_after, 0) AS TotalAccumulatedPoints,
                                    CASE WHEN u.role = @StudentRole THEN TRUE ELSE FALSE END AS IsGamificationParticipant,
                                    membership.ActiveMembershipPlanId,
                                    membership.ActiveMembershipPlanName,
                                    membership.MembershipBillingDueDay,
                                    membership.IsMembershipInGoodStanding
                             FROM users u
                             LEFT JOIN user_profiles up
                               ON up.user_id = u.id
                              AND up.tenant_id = u.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT balance_after
                                 FROM points_ledger
                                 WHERE tenant_id = u.tenant_id
                                   AND user_id = u.id
                                 ORDER BY created_at DESC, id DESC
                                 LIMIT 1
                             ) balance ON true
                             LEFT JOIN LATERAL (
                                 SELECT membership_base.membership_plan_id AS ActiveMembershipPlanId,
                                        membership_base.plan_name AS ActiveMembershipPlanName,
                                        membership_base.billing_due_day AS MembershipBillingDueDay,
                                        CASE
                                            WHEN membership_base.billing_due_day IS NULL THEN NULL
                                            WHEN membership_base.current_due_date >= @TodayDate THEN TRUE
                                            WHEN membership_base.is_paid_on_time IS TRUE THEN TRUE
                                            ELSE FALSE
                                        END AS IsMembershipInGoodStanding
                                 FROM (
                                     SELECT um.membership_plan_id,
                                            mp.name AS plan_name,
                                            um.billing_due_day,
                                            payment.is_paid_on_time,
                                            make_date(
                                                EXTRACT(YEAR FROM @TodayDate)::int,
                                                EXTRACT(MONTH FROM @TodayDate)::int,
                                                LEAST(
                                                    COALESCE(um.billing_due_day, 1),
                                                    EXTRACT(DAY FROM ((date_trunc('month', @TodayDate::timestamp) + interval '1 month - 1 day'))::date)::int
                                                )
                                            ) AS current_due_date
                                     FROM user_memberships um
                                     JOIN membership_plans mp
                                       ON mp.id = um.membership_plan_id
                                      AND mp.tenant_id = um.tenant_id
                                     LEFT JOIN LATERAL (
                                         SELECT is_paid_on_time
                                         FROM membership_payments
                                         WHERE tenant_id = um.tenant_id
                                           AND user_id = um.user_id
                                           AND user_membership_id = um.id
                                           AND due_date::date = make_date(
                                               EXTRACT(YEAR FROM @TodayDate)::int,
                                               EXTRACT(MONTH FROM @TodayDate)::int,
                                               LEAST(
                                                   COALESCE(um.billing_due_day, 1),
                                                   EXTRACT(DAY FROM ((date_trunc('month', @TodayDate::timestamp) + interval '1 month - 1 day'))::date)::int
                                               )
                                           )
                                         ORDER BY created_at DESC, id DESC
                                         LIMIT 1
                                     ) payment ON true
                                     WHERE um.tenant_id = u.tenant_id
                                       AND um.user_id = u.id
                                       AND um.is_active = true
                                       AND um.starts_at <= @TodayUtc
                                       AND (um.ends_at IS NULL OR um.ends_at > @TodayUtc)
                                     ORDER BY um.starts_at DESC, um.created_at DESC
                                     LIMIT 1
                                 ) membership_base
                             ) membership ON true
                             WHERE u.tenant_id = @TenantId
                             ORDER BY u.name";

        var users = await connection.QueryAsync<UserResponse>(sql, new { TenantId = tenantId, StudentRole = UserRole.Student, TodayUtc = todayUtc, TodayDate = todayDate });

        return Results.Ok(users);
    }

    /// <summary>
    /// Retorna um usuário específico do tenant atual.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var user = await GetUserResponseAsync(id, db, tenantId);

        if (user is null)
            return Results.NotFound();

        return Results.Ok(user);
    }

    /// <summary>
    /// Cria um novo usuário no tenant autenticado.
    /// O e-mail é normalizado para comparação consistente e a senha já nasce com hash seguro.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateUserRequest request,
                                                   AppDbContext db,
                                                   IBCryptPasswordHasher passwordHasher,
                                                   IGamificationService gamificationService,
                                                   HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return Results.BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest("Password is required.");

        var normalizedPhoneNumber = NormalizePhoneNumber(request.PhoneNumber);
        var normalizedBirthDate = NormalizeBirthDate(request.BirthDate);
        var profileValidation = ValidateProfileFields(normalizedPhoneNumber, normalizedBirthDate);
        if (profileValidation is not null)
            return profileValidation;

        var membershipValidation = ValidateMembershipFields(request.Role, request.MembershipPlanId, request.MembershipBillingDueDay);
        if (membershipValidation is not null)
            return membershipValidation;

        var tenantId = context.GetTenantId();
        var email = request.Email.Trim().ToLower();
        var actorUserId = GetUserId(context.User);

        var exists = await db.Users.AnyAsync(x => x.TenantId == tenantId && x.Email == email);

        if (exists)
            return Results.Conflict("A user with this e-mail already exists in this tenant.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);

        if (!string.IsNullOrWhiteSpace(normalizedPhoneNumber) || normalizedBirthDate.HasValue)
        {
            db.UserProfiles.Add(new UserProfile
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = user.Id,
                PhoneNumber = normalizedPhoneNumber,
                BirthDate = normalizedBirthDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        var membershipSync = await SyncMembershipAsync(
            db,
            gamificationService,
            tenantId,
            user.Id,
            actorUserId,
            request.Role,
            request.MembershipPlanId,
            request.MembershipBillingDueDay,
            "Plano vinculado no cadastro do usuário.",
            "Plano removido no cadastro do usuário.");

        if (membershipSync is not null)
            return membershipSync;

        await db.SaveChangesAsync();

        var response = await GetUserResponseAsync(user.Id, db, tenantId);
        if (response is null)
            return Results.Problem("User was created, but the response could not be generated.");

        return Results.Created($"/api/users/{user.Id}", response);
    }

    /// <summary>
    /// Atualiza os dados administrativos de um usuário existente do tenant.
    /// Nesta V1 a equipe pode alterar nome, role e status ativo.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id,
                                                   UpdateUserRequest request,
                                                   AppDbContext db,
                                                   IGamificationService gamificationService,
                                                   HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        var normalizedPhoneNumber = NormalizePhoneNumber(request.PhoneNumber);
        var normalizedBirthDate = NormalizeBirthDate(request.BirthDate);
        var profileValidation = ValidateProfileFields(normalizedPhoneNumber, normalizedBirthDate);
        if (profileValidation is not null)
            return profileValidation;

        var membershipValidation = ValidateMembershipFields(request.Role, request.MembershipPlanId, request.MembershipBillingDueDay);
        if (membershipValidation is not null)
            return membershipValidation;

        var tenantId = context.GetTenantId();
        var actorUserId = GetUserId(context.User);

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (user is null)
            return Results.NotFound();

        user.Name = request.Name.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        var profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id && x.TenantId == tenantId);
        if (profile is null && (!string.IsNullOrWhiteSpace(normalizedPhoneNumber) || normalizedBirthDate.HasValue))
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            db.UserProfiles.Add(profile);
        }

        if (profile is not null)
        {
            profile.PhoneNumber = normalizedPhoneNumber;
            profile.BirthDate = normalizedBirthDate;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        var membershipSync = await SyncMembershipAsync(
            db,
            gamificationService,
            tenantId,
            user.Id,
            actorUserId,
            request.Role,
            request.MembershipPlanId,
            request.MembershipBillingDueDay,
            "Plano ajustado na edição do usuário.",
            "Plano removido na edição do usuário.");

        if (membershipSync is not null)
            return membershipSync;

        await db.SaveChangesAsync();

        var response = await GetUserResponseAsync(user.Id, db, tenantId);
        if (response is null)
            return Results.Problem("User was updated, but the response could not be generated.");

        return Results.Ok(response);
    }

    /// <summary>
    /// Desativa logicamente um usuário do tenant atual.
    /// O registro permanece no banco para preservar histórico e relacionamentos.
    /// </summary>
    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (user is null)
            return Results.NotFound();

        user.IsActive = false;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Retorna o histórico de pontuação individual do usuário consultado pela gestão.
    /// </summary>
    private static async Task<IResult> GetGamificationHistoryAsync(Guid id, AppDbContext db, HttpContext context, int limit = 100)
    {
        var tenantId = context.GetTenantId();
        limit = limit is < 1 or > 200 ? 100 : limit;

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (user is null)
            return Results.NotFound();

        var totalAccumulatedPoints = await db.PointsLedgers
            .Where(x => x.TenantId == tenantId && x.UserId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => (decimal?)x.BalanceAfter)
            .FirstOrDefaultAsync() ?? 0m;

        if (user.Role != UserRole.Student)
        {
            return Results.Ok(new UserGamificationHistoryResponse(
                user.Id,
                user.Name,
                false,
                totalAccumulatedPoints,
                []));
        }

        var events = await db.UserGamificationEvents
            .Where(x => x.TenantId == tenantId && x.UserId == id)
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(limit)
            .Select(x => new UserGamificationHistoryProjection(
                x.Id,
                x.EventType,
                x.GamificationEventRule.Name,
                x.PointsApplied,
                x.SourceEntity,
                x.SourceEntityId,
                x.OccurredAt,
                x.Notes))
            .ToListAsync();

        var sourceTitles = await BuildSourceTitleMapAsync(db, tenantId, events);
        var entries = events
            .Select(x => new UserGamificationHistoryEntryResponse(
                x.Id,
                x.EventType,
                x.RuleName,
                x.PointsApplied,
                x.SourceEntity,
                ToSourceLabel(x.SourceEntity),
                x.SourceEntityId,
                x.SourceEntityId.HasValue && sourceTitles.TryGetValue(x.SourceEntityId.Value, out var sourceTitle) ? sourceTitle : null,
                x.OccurredAt,
                x.Notes))
            .ToList();

        return Results.Ok(new UserGamificationHistoryResponse(
            user.Id,
            user.Name,
            true,
            totalAccumulatedPoints,
            entries));
    }

    private static async Task<Dictionary<Guid, string>> BuildSourceTitleMapAsync(AppDbContext db,
                                                                                  Guid tenantId,
                                                                                  List<UserGamificationHistoryProjection> events)
    {
        var titles = new Dictionary<Guid, string>();

        var checkInIds = events.Where(x => x.SourceEntity == "checkin" && x.SourceEntityId.HasValue).Select(x => x.SourceEntityId!.Value).Distinct().ToList();
        if (checkInIds.Count > 0)
        {
            var checkins = await db.CheckIns
                .Where(x => x.TenantId == tenantId && checkInIds.Contains(x.Id))
                .Select(x => new { x.Id, x.CheckedInAt })
                .ToListAsync();

            foreach (var checkin in checkins)
                titles[checkin.Id] = $"Check-in de {checkin.CheckedInAt.ToLocalTime():dd/MM/yyyy HH:mm}";
        }

        var classBookingIds = events.Where(x => x.SourceEntity == "class_booking" && x.SourceEntityId.HasValue).Select(x => x.SourceEntityId!.Value).Distinct().ToList();
        if (classBookingIds.Count > 0)
        {
            var bookings = await db.Set<ClassBooking>()
                .Where(x => x.TenantId == tenantId && classBookingIds.Contains(x.Id))
                .Select(x => new { x.Id, ClassName = x.GymClass.Name, x.GymClass.StartsAt })
                .ToListAsync();

            foreach (var booking in bookings)
                titles[booking.Id] = $"{booking.ClassName} ({booking.StartsAt.ToLocalTime():dd/MM HH:mm})";
        }

        var participationIds = events.Where(x => x.SourceEntity == "academy_event_completion" && x.SourceEntityId.HasValue).Select(x => x.SourceEntityId!.Value).Distinct().ToList();
        if (participationIds.Count > 0)
        {
            var participations = await db.Set<AcademyEventParticipation>()
                .Where(x => x.TenantId == tenantId && participationIds.Contains(x.Id))
                .Select(x => new { x.Id, EventTitle = x.AcademyEvent.Title })
                .ToListAsync();

            foreach (var participation in participations)
                titles[participation.Id] = participation.EventTitle;
        }

        var socialPostIds = events.Where(x => x.SourceEntity == "social_post" && x.SourceEntityId.HasValue).Select(x => x.SourceEntityId!.Value).Distinct().ToList();
        if (socialPostIds.Count > 0)
        {
            var posts = await db.Set<SocialPost>()
                .Where(x => x.TenantId == tenantId && socialPostIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Description })
                .ToListAsync();

            foreach (var post in posts)
                titles[post.Id] = string.IsNullOrWhiteSpace(post.Description)
                    ? "Publicação social"
                    : Truncate(post.Description, 80);
        }

        var storeOrderIds = events.Where(x => x.SourceEntity == "store_order" && x.SourceEntityId.HasValue).Select(x => x.SourceEntityId!.Value).Distinct().ToList();
        if (storeOrderIds.Count > 0)
        {
            var orders = await db.Set<StoreOrder>()
                .Where(x => x.TenantId == tenantId && storeOrderIds.Contains(x.Id))
                .Select(x => new { x.Id, x.CreatedAt, x.TotalAmount })
                .ToListAsync();

            foreach (var order in orders)
                titles[order.Id] = $"Pedido da loja de {order.CreatedAt.ToLocalTime():dd/MM/yyyy} ({order.TotalAmount:C})";
        }

        var membershipIds = events.Where(x => x.SourceEntity == "user_membership" && x.SourceEntityId.HasValue).Select(x => x.SourceEntityId!.Value).Distinct().ToList();
        if (membershipIds.Count > 0)
        {
            var memberships = await db.Set<UserMembership>()
                .Where(x => x.TenantId == tenantId && membershipIds.Contains(x.Id))
                .Select(x => new { x.Id, PlanName = x.MembershipPlan.Name })
                .ToListAsync();

            foreach (var membership in memberships)
                titles[membership.Id] = membership.PlanName;
        }

        var paymentIds = events.Where(x => x.SourceEntity == "membership_payment" && x.SourceEntityId.HasValue).Select(x => x.SourceEntityId!.Value).Distinct().ToList();
        if (paymentIds.Count > 0)
        {
            var payments = await db.Set<MembershipPayment>()
                .Where(x => x.TenantId == tenantId && paymentIds.Contains(x.Id))
                .Select(x => new { x.Id, PlanName = x.MembershipPlan.Name, x.PaidAt })
                .ToListAsync();

            foreach (var payment in payments)
                titles[payment.Id] = $"{payment.PlanName} ({payment.PaidAt.ToLocalTime():dd/MM/yyyy})";
        }

        return titles;
    }

    private static IResult? ValidateProfileFields(string? phoneNumber, DateTime? birthDate)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var digits = GetPhoneDigits(phoneNumber);
            if (digits.Length < 10 || digits.Length > 11)
                return Results.BadRequest("Phone number must include area code and a valid Brazilian mobile or landline number.");
        }

        if (birthDate.HasValue)
        {
            if (birthDate.Value.Date > DateTime.UtcNow.Date)
                return Results.BadRequest("Birth date cannot be in the future.");

            if (birthDate.Value.Date < new DateTime(1900, 1, 1))
                return Results.BadRequest("Birth date is invalid.");
        }

        return null;
    }

    private static IResult? ValidateMembershipFields(UserRole role, Guid? membershipPlanId, int? membershipBillingDueDay)
    {
        if (role != UserRole.Student)
            return null;

        if (!membershipPlanId.HasValue && !membershipBillingDueDay.HasValue)
            return null;

        if (!membershipPlanId.HasValue)
            return Results.BadRequest("Select a membership plan before informing the due day.");

        if (!membershipBillingDueDay.HasValue)
            return Results.BadRequest("Membership due day is required when a plan is selected.");

        if (membershipBillingDueDay.Value < 1 || membershipBillingDueDay.Value > 31)
            return Results.BadRequest("Membership due day must be between 1 and 31.");

        return null;
    }

    private static async Task<UserResponse?> GetUserResponseAsync(Guid userId, AppDbContext db, Guid tenantId)
    {
        var connection = db.Database.GetDbConnection();
        var todayUtc = DateTime.UtcNow;
        var todayDate = todayUtc.Date;

        const string sql = @"SELECT u.id,
                                    u.tenant_id AS TenantId,
                                    u.name,
                                    u.email,
                                    u.role,
                                    u.is_active AS IsActive,
                                    u.created_at AS CreatedAt,
                                    up.phone_number AS PhoneNumber,
                                    CASE
                                        WHEN up.birth_date IS NULL THEN NULL
                                        ELSE TO_CHAR(up.birth_date, 'YYYY-MM-DD')
                                    END AS BirthDate,
                                    COALESCE(balance.balance_after, 0) AS TotalAccumulatedPoints,
                                    CASE WHEN u.role = @StudentRole THEN TRUE ELSE FALSE END AS IsGamificationParticipant,
                                    membership.ActiveMembershipPlanId,
                                    membership.ActiveMembershipPlanName,
                                    membership.MembershipBillingDueDay,
                                    membership.IsMembershipInGoodStanding
                             FROM users u
                             LEFT JOIN user_profiles up
                               ON up.user_id = u.id
                              AND up.tenant_id = u.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT balance_after
                                 FROM points_ledger
                                 WHERE tenant_id = u.tenant_id
                                   AND user_id = u.id
                                 ORDER BY created_at DESC, id DESC
                                 LIMIT 1
                             ) balance ON true
                             LEFT JOIN LATERAL (
                                 SELECT membership_base.membership_plan_id AS ActiveMembershipPlanId,
                                        membership_base.plan_name AS ActiveMembershipPlanName,
                                        membership_base.billing_due_day AS MembershipBillingDueDay,
                                        CASE
                                            WHEN membership_base.billing_due_day IS NULL THEN NULL
                                            WHEN membership_base.current_due_date >= @TodayDate THEN TRUE
                                            WHEN membership_base.is_paid_on_time IS TRUE THEN TRUE
                                            ELSE FALSE
                                        END AS IsMembershipInGoodStanding
                                 FROM (
                                     SELECT um.membership_plan_id,
                                            mp.name AS plan_name,
                                            um.billing_due_day,
                                            payment.is_paid_on_time,
                                            make_date(
                                                EXTRACT(YEAR FROM @TodayDate)::int,
                                                EXTRACT(MONTH FROM @TodayDate)::int,
                                                LEAST(
                                                    COALESCE(um.billing_due_day, 1),
                                                    EXTRACT(DAY FROM ((date_trunc('month', @TodayDate::timestamp) + interval '1 month - 1 day'))::date)::int
                                                )
                                            ) AS current_due_date
                                     FROM user_memberships um
                                     JOIN membership_plans mp
                                       ON mp.id = um.membership_plan_id
                                      AND mp.tenant_id = um.tenant_id
                                     LEFT JOIN LATERAL (
                                         SELECT is_paid_on_time
                                         FROM membership_payments
                                         WHERE tenant_id = um.tenant_id
                                           AND user_id = um.user_id
                                           AND user_membership_id = um.id
                                           AND due_date::date = make_date(
                                               EXTRACT(YEAR FROM @TodayDate)::int,
                                               EXTRACT(MONTH FROM @TodayDate)::int,
                                               LEAST(
                                                   COALESCE(um.billing_due_day, 1),
                                                   EXTRACT(DAY FROM ((date_trunc('month', @TodayDate::timestamp) + interval '1 month - 1 day'))::date)::int
                                               )
                                           )
                                         ORDER BY created_at DESC, id DESC
                                         LIMIT 1
                                     ) payment ON true
                                     WHERE um.tenant_id = u.tenant_id
                                       AND um.user_id = u.id
                                       AND um.is_active = true
                                       AND um.starts_at <= @TodayUtc
                                       AND (um.ends_at IS NULL OR um.ends_at > @TodayUtc)
                                     ORDER BY um.starts_at DESC, um.created_at DESC
                                     LIMIT 1
                                 ) membership_base
                             ) membership ON true
                             WHERE u.id = @Id AND u.tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<UserResponse>(sql, new
        {
            Id = userId,
            TenantId = tenantId,
            StudentRole = UserRole.Student,
            TodayUtc = todayUtc,
            TodayDate = todayDate
        });
    }

    private static async Task<IResult?> SyncMembershipAsync(AppDbContext db,
                                                            IGamificationService gamificationService,
                                                            Guid tenantId,
                                                            Guid userId,
                                                            Guid actorUserId,
                                                            UserRole role,
                                                            Guid? membershipPlanId,
                                                            int? membershipBillingDueDay,
                                                            string updateReason,
                                                            string removalReason)
    {
        var now = DateTime.UtcNow;
        var currentMemberships = await db.UserMemberships
            .Where(x => x.UserId == userId && x.TenantId == tenantId && x.IsActive)
            .OrderByDescending(x => x.StartsAt)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

        if (role != UserRole.Student || !membershipPlanId.HasValue)
        {
            foreach (var membership in currentMemberships)
            {
                membership.IsActive = false;
                if (!membership.EndsAt.HasValue || membership.EndsAt > now)
                    membership.EndsAt = now;
                membership.StatusReason = removalReason;
                membership.ChangedByUserId = actorUserId;
            }

            return null;
        }

        var plan = await db.MembershipPlans.FirstOrDefaultAsync(x => x.Id == membershipPlanId.Value && x.TenantId == tenantId && x.IsActive);
        if (plan is null)
            return Results.BadRequest("Selected membership plan is not available for this academy.");

        var currentActiveMembership = currentMemberships.FirstOrDefault(x =>
            x.MembershipPlanId == membershipPlanId.Value &&
            x.StartsAt <= now &&
            (!x.EndsAt.HasValue || x.EndsAt > now));

        if (currentActiveMembership is not null)
        {
            currentActiveMembership.BillingDueDay = membershipBillingDueDay;
            currentActiveMembership.ChangedByUserId = actorUserId;
            currentActiveMembership.StatusReason = updateReason;
            return null;
        }

        var hadHistoricalMembership = currentMemberships.Count > 0 || await db.UserMemberships
            .AnyAsync(x => x.UserId == userId && x.TenantId == tenantId);

        foreach (var membership in currentMemberships)
        {
            membership.IsActive = false;
            if (!membership.EndsAt.HasValue || membership.EndsAt > now)
                membership.EndsAt = now;
            membership.StatusReason = updateReason;
            membership.ChangedByUserId = actorUserId;
        }

        var newMembership = new UserMembership
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            MembershipPlanId = membershipPlanId.Value,
            StartsAt = now,
            EndsAt = now.AddDays(plan.DurationDays),
            IsActive = true,
            BillingDueDay = membershipBillingDueDay,
            StatusReason = updateReason,
            ChangedByUserId = actorUserId,
            CreatedAt = now
        };

        db.UserMemberships.Add(newMembership);

        if (hadHistoricalMembership)
        {
            await gamificationService.AwardEventAsync(
                tenantId,
                userId,
                GamificationEventType.PlanRenewal,
                "user_membership",
                newMembership.Id,
                now,
                updateReason);
        }

        return null;
    }

    private static string? NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var digits = GetPhoneDigits(phoneNumber);
        if (digits.Length == 11)
            return $"({digits[..2]}) {digits.Substring(2, 5)}-{digits.Substring(7, 4)}";

        if (digits.Length == 10)
            return $"({digits[..2]}) {digits.Substring(2, 4)}-{digits.Substring(6, 4)}";

        return phoneNumber.Trim();
    }

    private static DateTime? NormalizeBirthDate(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return null;

        var acceptedFormats = new[] { "dd/MM/yyyy", "yyyy-MM-dd" };
        if (!DateTime.TryParseExact(birthDate.Trim(), acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return DateTime.MinValue;

        return DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
    }

    private static string GetPhoneDigits(string phoneNumber)
    {
        return Regex.Replace(phoneNumber, "[^0-9]", string.Empty);
    }

    private static string ToSourceLabel(string sourceEntity)
    {
        return sourceEntity switch
        {
            "checkin" => "Check-in",
            "class_booking" => "Aula especial",
            "academy_event_completion" => "Evento",
            "social_post" => "Publicação",
            "store_order" => "Loja",
            "user_membership" => "Renovação de plano",
            "membership_payment" => "Pagamento em dia",
            _ => "Atividade"
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            return value;

        return $"{value[..maxLength].TrimEnd()}...";
    }

    private sealed record UserGamificationHistoryProjection(
        Guid Id,
        GamificationEventType EventType,
        string RuleName,
        decimal PointsApplied,
        string SourceEntity,
        Guid? SourceEntityId,
        DateTime OccurredAt,
        string? Notes
    );

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User not found in token.");

        return Guid.Parse(userIdClaim);
    }
}
