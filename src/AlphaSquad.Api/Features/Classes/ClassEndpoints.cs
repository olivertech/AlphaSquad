using AlphaSquad.Shared.Enums;

namespace AlphaSquad.Api.Features.Classes;

public static class ClassEndpoints
{
    /// <summary>
    /// Registra os endpoints do mÃ³dulo de aulas e agendas.
    /// Este grupo concentra o CRUD bÃ¡sico das aulas do tenant.
    /// </summary>
    public static IEndpointRouteBuilder MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes")
            .WithTags("Classes")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetClasses")
            .WithSummary("Lista as aulas do tenant atual.")
            .WithDescription("Retorna uma lista paginada de aulas com filtros por perÃ­odo e status ativo.")
            .Produces<PagedResponse<GymClassResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetClassById")
            .WithSummary("Busca uma aula especÃ­fica do tenant atual.")
            .WithDescription("Retorna o detalhamento de uma aula pelo identificador, incluindo dados do instrutor quando existir.")
            .Produces<GymClassResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateClass")
            .WithSummary("Cria uma nova aula no tenant atual.")
            .WithDescription("Cadastra uma aula com horÃ¡rio, capacidade, local e instrutor opcional, restrito a perfis de gestÃ£o.")
            .Produces<GymClassResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateClass")
            .WithSummary("Atualiza uma aula do tenant atual.")
            .WithDescription("Permite alterar os dados principais da aula, incluindo status ativo e instrutor.")
            .Produces<GymClassResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteClass")
            .WithSummary("Remove uma aula do tenant atual.")
            .WithDescription("Exclui fisicamente uma aula pelo identificador, restrito a perfis com permissÃ£o de gestÃ£o.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/book", BookAsync)
            .WithName("BookClass")
            .WithSummary("Reserva uma vaga em uma aula.")
            .WithDescription("Cria a reserva da aula para o usuÃ¡rio autenticado, respeitando capacidade, duplicidade e horÃ¡rio da aula.")
            .Produces<ClassBookingResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/bookings", CreateBookingForUserAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateClassBookingForUser")
            .WithSummary("Reserva uma vaga em uma aula para um aluno específico.")
            .WithDescription("Permite que a gestão da academia confirme uma reserva em nome de um aluno, útil para recepção e atendimento presencial.")
            .Produces<ClassBookingResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/bookings/{bookingId:guid}", DeleteBookingForUserAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteClassBookingForUser")
            .WithSummary("Remove a reserva de um aluno em uma aula.")
            .WithDescription("Permite que a gestão da academia cancele uma reserva feita para um aluno, útil para ajustes operacionais e atendimento presencial.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/book", UnbookAsync)
            .WithName("UnbookClass")
            .WithSummary("Cancela a reserva da aula para o usuÃ¡rio autenticado.")
            .WithDescription("Remove a reserva existente do usuÃ¡rio para a aula informada dentro do tenant atual.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/bookings", GetBookingsByClassAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetClassBookingsByClass")
            .WithSummary("Lista as reservas de uma aula para a equipe da academia.")
            .WithDescription("Retorna as reservas da aula informada, com dados do aluno e ordenacao por data de reserva.")
            .Produces<List<ClassBookingManagementResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/bookings/by-user/{userId:guid}", GetBookingsByUserAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetClassBookingsByUser")
            .WithSummary("Lista as reservas de aula de um aluno para a equipe da academia.")
            .WithDescription("Retorna as reservas vinculadas ao aluno informado dentro do tenant atual, com filtros opcionais por status e periodo.")
            .Produces<List<ClassBookingManagementResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Retorna a listagem de aulas do tenant atual com paginaÃ§Ã£o e filtro por perÃ­odo.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db,
                                                   HttpContext context,
                                                   DateTime? dateFrom = null,
                                                   DateTime? dateTo = null,
                                                   bool? isActive = null,
                                                   int page = 1,
                                                   int pageSize = 20)
    {
        var tenantId = context.GetTenantId();
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var connection = db.Database.GetDbConnection();
        var dateToExclusive = dateTo?.Date.AddDays(1);

        const string countSql = @"SELECT COUNT(*)
                                  FROM gym_classes gc
                                  WHERE gc.tenant_id = @TenantId
                                    AND (CAST(@IsActive AS boolean) IS NULL OR gc.is_active = @IsActive)
                                    AND (CAST(@DateFrom AS timestamptz) IS NULL OR gc.starts_at >= @DateFrom)
                                    AND (CAST(@DateToExclusive AS timestamptz) IS NULL OR gc.starts_at < @DateToExclusive)";

        const string itemsSql = @"SELECT gc.id,
                                         gc.name,
                                         gc.description,
                                         gc.instructor_user_id AS InstructorUserId,
                                         u.name AS InstructorName,
                                         gc.starts_at AS StartsAt,
                                         gc.ends_at AS EndsAt,
                                         gc.location,
                                         gc.capacity,
                                         gc.is_active AS IsActive,
                                         gc.is_special_class AS IsSpecialClass,
                                         gc.created_at AS CreatedAt
                                  FROM gym_classes gc
                                  LEFT JOIN users u ON u.id = gc.instructor_user_id AND u.tenant_id = @TenantId
                                  WHERE gc.tenant_id = @TenantId
                                    AND (CAST(@IsActive AS boolean) IS NULL OR gc.is_active = @IsActive)
                                    AND (CAST(@DateFrom AS timestamptz) IS NULL OR gc.starts_at >= @DateFrom)
                                    AND (CAST(@DateToExclusive AS timestamptz) IS NULL OR gc.starts_at < @DateToExclusive)
                                  ORDER BY gc.starts_at ASC
                                  LIMIT @Limit OFFSET @Offset";

        var parameters = new
        {
            TenantId = tenantId,
            IsActive = isActive,
            DateFrom = dateFrom,
            DateToExclusive = dateToExclusive,
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<GymClassResponse>(itemsSql, parameters);

        return Results.Ok(new PagedResponse<GymClassResponse>(page, pageSize, total, items.ToList()));
    }

    /// <summary>
    /// Retorna o detalhamento de uma aula especÃ­fica do tenant atual.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT gc.id,
                                    gc.name,
                                    gc.description,
                                    gc.instructor_user_id AS InstructorUserId,
                                    u.name AS InstructorName,
                                    gc.starts_at AS StartsAt,
                                    gc.ends_at AS EndsAt,
                                    gc.location,
                                    gc.capacity,
                                    gc.is_active AS IsActive,
                                    gc.is_special_class AS IsSpecialClass,
                                    gc.created_at AS CreatedAt
                             FROM gym_classes gc
                             LEFT JOIN users u ON u.id = gc.instructor_user_id AND u.tenant_id = @TenantId
                             WHERE gc.id = @Id AND gc.tenant_id = @TenantId";

        var gymClass = await connection.QueryFirstOrDefaultAsync<GymClassResponse>(sql, new { Id = id, TenantId = tenantId });

        if (gymClass is null)
            return Results.NotFound();

        return Results.Ok(gymClass);
    }

    /// <summary>
    /// Cria uma nova aula para o tenant atual.
    /// Apenas perfis administrativos ou professores podem gerenciar aulas.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateGymClassRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var normalizedStartsAt = NormalizeToUtc(request.StartsAt);
        var normalizedEndsAt = NormalizeToUtc(request.EndsAt);

        var validation = await ValidateRequestAsync(request.Name, normalizedStartsAt, normalizedEndsAt, request.Capacity, request.InstructorUserId, tenantId, db);
        if (validation is not null)
            return validation;

        var gymClass = new GymClass
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            InstructorUserId = request.InstructorUserId,
            StartsAt = normalizedStartsAt,
            EndsAt = normalizedEndsAt,
            Location = NormalizeOptional(request.Location),
            Capacity = request.Capacity,
            IsActive = true,
            IsSpecialClass = request.IsSpecialClass,
            CreatedAt = DateTime.UtcNow
        };

        db.Set<GymClass>().Add(gymClass);
        await db.SaveChangesAsync();

        var response = await BuildResponseAsync(gymClass.Id, tenantId, db);
        return Results.Created($"/api/classes/{gymClass.Id}", response);
    }

    /// <summary>
    /// Atualiza uma aula existente do tenant atual.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id, UpdateGymClassRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var gymClass = await db.Set<GymClass>().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (gymClass is null)
            return Results.NotFound();

        var normalizedStartsAt = NormalizeToUtc(request.StartsAt);
        var normalizedEndsAt = NormalizeToUtc(request.EndsAt);

        var validation = await ValidateRequestAsync(request.Name, normalizedStartsAt, normalizedEndsAt, request.Capacity, request.InstructorUserId, tenantId, db);
        if (validation is not null)
            return validation;

        gymClass.Name = request.Name.Trim();
        gymClass.Description = NormalizeOptional(request.Description);
        gymClass.InstructorUserId = request.InstructorUserId;
        gymClass.StartsAt = normalizedStartsAt;
        gymClass.EndsAt = normalizedEndsAt;
        gymClass.Location = NormalizeOptional(request.Location);
        gymClass.Capacity = request.Capacity;
        gymClass.IsActive = request.IsActive;
        gymClass.IsSpecialClass = request.IsSpecialClass;

        await db.SaveChangesAsync();

        var response = await BuildResponseAsync(gymClass.Id, tenantId, db);
        return Results.Ok(response);
    }

    /// <summary>
    /// Remove uma aula do tenant atual.
    /// Neste primeiro recorte, a exclusÃ£o Ã© fÃ­sica.
    /// </summary>
    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var gymClass = await db.Set<GymClass>().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (gymClass is null)
            return Results.NotFound();

        db.Set<GymClass>().Remove(gymClass);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Cria uma reserva para o usuÃ¡rio autenticado em uma aula do tenant atual.
    /// </summary>
    private static async Task<IResult> BookAsync(Guid id,
                                                 AppDbContext db,
                                                 IGamificationService gamificationService,
                                                 HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var appUser = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (appUser is null)
            return Results.Unauthorized();

        var gymClass = await db.GymClasses.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.IsActive);
        if (gymClass is null)
            return Results.NotFound();

        if (gymClass.StartsAt <= DateTime.UtcNow)
            return Results.BadRequest("Class has already started or finished.");

        var alreadyBooked = await db.ClassBookings.AnyAsync(x => x.GymClassId == id && x.UserId == userId && x.TenantId == tenantId);
        if (alreadyBooked)
            return Results.Conflict("User is already booked for this class.");

        var currentBookings = await db.ClassBookings.CountAsync(x => x.GymClassId == id && x.TenantId == tenantId);
        if (currentBookings >= gymClass.Capacity)
            return Results.BadRequest("Class is already full.");

        var booking = new ClassBooking
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            GymClassId = id,
            UserId = userId,
            BookedAt = DateTime.UtcNow
        };

        db.ClassBookings.Add(booking);
        await db.SaveChangesAsync();

        if (gymClass.IsSpecialClass)
        {
            await gamificationService.AwardEventAsync(
                tenantId,
                userId,
                GamificationEventType.ClassSpecialParticipation,
                "class_booking",
                booking.Id,
                booking.BookedAt,
                "Special class booking processed successfully.");
        }

        return Results.Created($"/api/classes/{id}/book", new ClassBookingResponse(
            booking.Id,
            gymClass.Id,
            gymClass.Name,
            appUser.Id,
            appUser.Name,
            booking.BookedAt
        ));
    }

    /// <summary>
    /// Remove a reserva do usuÃ¡rio autenticado para uma aula do tenant atual.
    /// </summary>
    /// <summary>
    /// Cria uma reserva administrativa para um aluno específico.
    /// Esse fluxo foi pensado para recepção e atendimento presencial, onde a equipe confirma a vaga pelo painel.
    /// </summary>
    private static async Task<IResult> CreateBookingForUserAsync(Guid id,
                                                                 CreateClassBookingForUserRequest request,
                                                                 AppDbContext db,
                                                                 IGamificationService gamificationService,
                                                                 HttpContext context)
    {
        var tenantId = context.GetTenantId();

        if (request.UserId == Guid.Empty)
            return Results.BadRequest("UserId is required.");

        var appUser = await db.Users.FirstOrDefaultAsync(x => x.Id == request.UserId && x.TenantId == tenantId && x.IsActive);
        if (appUser is null)
            return Results.BadRequest("User does not belong to this tenant or is inactive.");

        if (appUser.Role != UserRole.Student)
            return Results.BadRequest("Only students can be booked into classes.");

        var gymClass = await db.GymClasses.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.IsActive);
        if (gymClass is null)
            return Results.NotFound();

        if (gymClass.StartsAt <= DateTime.UtcNow)
            return Results.BadRequest("Class has already started or finished.");

        var alreadyBooked = await db.ClassBookings.AnyAsync(x => x.GymClassId == id && x.UserId == request.UserId && x.TenantId == tenantId);
        if (alreadyBooked)
            return Results.Conflict("User is already booked for this class.");

        var currentBookings = await db.ClassBookings.CountAsync(x => x.GymClassId == id && x.TenantId == tenantId);
        if (currentBookings >= gymClass.Capacity)
            return Results.BadRequest("Class is already full.");

        var booking = new ClassBooking
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            GymClassId = id,
            UserId = request.UserId,
            BookedAt = DateTime.UtcNow
        };

        db.ClassBookings.Add(booking);
        await db.SaveChangesAsync();

        if (gymClass.IsSpecialClass)
        {
            await gamificationService.AwardEventAsync(
                tenantId,
                request.UserId,
                GamificationEventType.ClassSpecialParticipation,
                "class_booking",
                booking.Id,
                booking.BookedAt,
                "Special class booking processed successfully.");
        }

        return Results.Created($"/api/classes/{id}/bookings", new ClassBookingResponse(
            booking.Id,
            gymClass.Id,
            gymClass.Name,
            appUser.Id,
            appUser.Name,
            booking.BookedAt
        ));
    }

    /// <summary>
    /// Remove administrativamente a reserva de um aluno em uma aula.
    /// Esse fluxo ajuda a recepção e a gestão a liberar vagas quando a reserva precisa ser cancelada manualmente.
    /// </summary>
    private static async Task<IResult> DeleteBookingForUserAsync(Guid id,
                                                                 Guid bookingId,
                                                                 AppDbContext db,
                                                                 HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var booking = await db.ClassBookings.FirstOrDefaultAsync(x =>
            x.Id == bookingId &&
            x.GymClassId == id &&
            x.TenantId == tenantId);

        if (booking is null)
            return Results.NotFound();

        db.ClassBookings.Remove(booking);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> UnbookAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var booking = await db.ClassBookings.FirstOrDefaultAsync(x => x.GymClassId == id && x.UserId == userId && x.TenantId == tenantId);
        if (booking is null)
            return Results.NotFound();

        db.ClassBookings.Remove(booking);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Retorna a lista operacional de reservas de uma aula especifica.
    /// Essa consulta ajuda a recepcao e a equipe a visualizar rapidamente quem reservou a turma.
    /// </summary>
    private static async Task<IResult> GetBookingsByClassAsync(Guid id,
                                                               AppDbContext db,
                                                               HttpContext context,
                                                               bool? onlyActiveClasses = null)
    {
        var tenantId = context.GetTenantId();
        var gymClassExists = await db.GymClasses.AnyAsync(x =>
            x.Id == id &&
            x.TenantId == tenantId &&
            (!onlyActiveClasses.HasValue || x.IsActive == onlyActiveClasses.Value));

        if (!gymClassExists)
            return Results.NotFound();

        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT cb.id AS BookingId,
                                    gc.id AS GymClassId,
                                    gc.name AS ClassName,
                                    gc.starts_at AS StartsAt,
                                    gc.ends_at AS EndsAt,
                                    gc.location,
                                    gc.is_special_class AS IsSpecialClass,
                                    u.id AS UserId,
                                    u.name AS UserName,
                                    u.email AS UserEmail,
                                    u.role AS UserRole,
                                    cb.booked_at AS BookedAt
                             FROM class_bookings cb
                             JOIN gym_classes gc
                               ON gc.id = cb.gym_class_id
                              AND gc.tenant_id = cb.tenant_id
                             JOIN users u
                               ON u.id = cb.user_id
                              AND u.tenant_id = cb.tenant_id
                             WHERE cb.tenant_id = @TenantId
                               AND cb.gym_class_id = @GymClassId
                             ORDER BY cb.booked_at ASC, u.name";

        var items = await connection.QueryAsync<ClassBookingManagementResponse>(sql, new
        {
            TenantId = tenantId,
            GymClassId = id
        });

        return Results.Ok(items.ToList());
    }

    /// <summary>
    /// Retorna a lista operacional de reservas de um aluno especifico.
    /// A equipe pode usar essa visao para acompanhar presenca planejada e agenda do aluno.
    /// </summary>
    private static async Task<IResult> GetBookingsByUserAsync(Guid userId,
                                                              AppDbContext db,
                                                              HttpContext context,
                                                              bool? onlyActiveClasses = null,
                                                              DateTime? dateFrom = null,
                                                              DateTime? dateTo = null)
    {
        var tenantId = context.GetTenantId();
        var userExists = await db.Users.AnyAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (!userExists)
            return Results.NotFound();

        var connection = db.Database.GetDbConnection();
        var dateToExclusive = dateTo?.Date.AddDays(1);

        const string sql = @"SELECT cb.id AS BookingId,
                                    gc.id AS GymClassId,
                                    gc.name AS ClassName,
                                    gc.starts_at AS StartsAt,
                                    gc.ends_at AS EndsAt,
                                    gc.location,
                                    gc.is_special_class AS IsSpecialClass,
                                    u.id AS UserId,
                                    u.name AS UserName,
                                    u.email AS UserEmail,
                                    u.role AS UserRole,
                                    cb.booked_at AS BookedAt
                             FROM class_bookings cb
                             JOIN gym_classes gc
                               ON gc.id = cb.gym_class_id
                              AND gc.tenant_id = cb.tenant_id
                             JOIN users u
                               ON u.id = cb.user_id
                              AND u.tenant_id = cb.tenant_id
                             WHERE cb.tenant_id = @TenantId
                               AND cb.user_id = @UserId
                               AND (CAST(@OnlyActiveClasses AS boolean) IS NULL OR gc.is_active = @OnlyActiveClasses)
                               AND (CAST(@DateFrom AS timestamptz) IS NULL OR gc.starts_at >= @DateFrom)
                               AND (CAST(@DateToExclusive AS timestamptz) IS NULL OR gc.starts_at < @DateToExclusive)
                             ORDER BY gc.starts_at ASC, cb.booked_at ASC";

        var items = await connection.QueryAsync<ClassBookingManagementResponse>(sql, new
        {
            TenantId = tenantId,
            UserId = userId,
            OnlyActiveClasses = onlyActiveClasses,
            DateFrom = dateFrom,
            DateToExclusive = dateToExclusive
        });

        return Results.Ok(items.ToList());
    }

    /// <summary>
    /// Valida as regras principais do payload antes de persistir a aula.
    /// </summary>
    private static async Task<IResult?> ValidateRequestAsync(string name,
                                                             DateTime startsAt,
                                                             DateTime endsAt,
                                                             int capacity,
                                                             Guid? instructorUserId,
                                                             Guid tenantId,
                                                             AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Class name is required.");

        if (endsAt <= startsAt)
            return Results.BadRequest("End date must be greater than start date.");

        if (capacity <= 0)
            return Results.BadRequest("Capacity must be greater than zero.");

        if (instructorUserId.HasValue)
        {
            var instructor = await db.Users.FirstOrDefaultAsync(x => x.Id == instructorUserId.Value && x.TenantId == tenantId && x.IsActive);
            if (instructor is null)
                return Results.BadRequest("Instructor does not belong to this tenant.");

            if (instructor.Role is not UserRole.Teacher and not UserRole.Admin)
                return Results.BadRequest("Instructor must have Teacher or Admin role.");
        }

        return null;
    }

    /// <summary>
    /// Recarrega a aula apÃ³s escrita para devolver a mesma projeÃ§Ã£o usada nas consultas.
    /// </summary>
    private static async Task<GymClassResponse> BuildResponseAsync(Guid id, Guid tenantId, AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT gc.id,
                                    gc.name,
                                    gc.description,
                                    gc.instructor_user_id AS InstructorUserId,
                                    u.name AS InstructorName,
                                    gc.starts_at AS StartsAt,
                                    gc.ends_at AS EndsAt,
                                    gc.location,
                                    gc.capacity,
                                    gc.is_active AS IsActive,
                                    gc.is_special_class AS IsSpecialClass,
                                    gc.created_at AS CreatedAt
                             FROM gym_classes gc
                             LEFT JOIN users u ON u.id = gc.instructor_user_id AND u.tenant_id = @TenantId
                             WHERE gc.id = @Id AND gc.tenant_id = @TenantId";

        return (await connection.QueryFirstAsync<GymClassResponse>(sql, new { Id = id, TenantId = tenantId }))!;
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

    /// <summary>
    /// Normaliza campos opcionais para evitar persistÃªncia de espaÃ§os em branco.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Converte datas recebidas pela API para UTC antes de persistir em colunas timestamptz.
    /// O dashboard trabalha com data e hora local, mas o PostgreSQL via Npgsql exige UTC no save.
    /// </summary>
    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
        };
    }
}

