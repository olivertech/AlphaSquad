using AlphaSquad.Shared.Enums;

namespace AlphaSquad.Api.Features.Classes;

public static class ClassEndpoints
{
    /// <summary>
    /// Registra os endpoints do módulo de aulas e agendas.
    /// Este grupo concentra o CRUD básico das aulas do tenant.
    /// </summary>
    public static IEndpointRouteBuilder MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes")
            .WithTags("Classes")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetClasses")
            .WithSummary("Lista as aulas do tenant atual.")
            .WithDescription("Retorna uma lista paginada de aulas com filtros por período e status ativo.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetClassById")
            .WithSummary("Busca uma aula específica do tenant atual.")
            .WithDescription("Retorna o detalhamento de uma aula pelo identificador, incluindo dados do instrutor quando existir.")
            .Produces<GymClassResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateClass")
            .WithSummary("Cria uma nova aula no tenant atual.")
            .WithDescription("Cadastra uma aula com horário, capacidade, local e instrutor opcional, restrito a perfis de gestão.")
            .Produces<GymClassResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateClass")
            .WithSummary("Atualiza uma aula do tenant atual.")
            .WithDescription("Permite alterar os dados principais da aula, incluindo status ativo e instrutor.")
            .Produces<GymClassResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteClass")
            .WithSummary("Remove uma aula do tenant atual.")
            .WithDescription("Exclui fisicamente uma aula pelo identificador, restrito a perfis com permissão de gestão.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/book", BookAsync)
            .WithName("BookClass")
            .WithSummary("Reserva uma vaga em uma aula.")
            .WithDescription("Cria a reserva da aula para o usuário autenticado, respeitando capacidade, duplicidade e horário da aula.")
            .Produces<ClassBookingResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/book", UnbookAsync)
            .WithName("UnbookClass")
            .WithSummary("Cancela a reserva da aula para o usuário autenticado.")
            .WithDescription("Remove a reserva existente do usuário para a aula informada dentro do tenant atual.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Retorna a listagem de aulas do tenant atual com paginação e filtro por período.
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
                                    AND (@IsActive IS NULL OR gc.is_active = @IsActive)
                                    AND (@DateFrom IS NULL OR gc.starts_at >= @DateFrom)
                                    AND (@DateToExclusive IS NULL OR gc.starts_at < @DateToExclusive)";

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
                                         gc.created_at AS CreatedAt
                                  FROM gym_classes gc
                                  LEFT JOIN users u ON u.id = gc.instructor_user_id AND u.tenant_id = @TenantId
                                  WHERE gc.tenant_id = @TenantId
                                    AND (@IsActive IS NULL OR gc.is_active = @IsActive)
                                    AND (@DateFrom IS NULL OR gc.starts_at >= @DateFrom)
                                    AND (@DateToExclusive IS NULL OR gc.starts_at < @DateToExclusive)
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

        return Results.Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }

    /// <summary>
    /// Retorna o detalhamento de uma aula específica do tenant atual.
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
        if (!CanManageClasses(context.User))
            return Results.Forbid();

        var tenantId = context.GetTenantId();
        var validation = await ValidateRequestAsync(request.Name, request.StartsAt, request.EndsAt, request.Capacity, request.InstructorUserId, tenantId, db);
        if (validation is not null)
            return validation;

        var gymClass = new GymClass
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            InstructorUserId = request.InstructorUserId,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Location = NormalizeOptional(request.Location),
            Capacity = request.Capacity,
            IsActive = true,
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
        if (!CanManageClasses(context.User))
            return Results.Forbid();

        var tenantId = context.GetTenantId();
        var gymClass = await db.Set<GymClass>().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (gymClass is null)
            return Results.NotFound();

        var validation = await ValidateRequestAsync(request.Name, request.StartsAt, request.EndsAt, request.Capacity, request.InstructorUserId, tenantId, db);
        if (validation is not null)
            return validation;

        gymClass.Name = request.Name.Trim();
        gymClass.Description = NormalizeOptional(request.Description);
        gymClass.InstructorUserId = request.InstructorUserId;
        gymClass.StartsAt = request.StartsAt;
        gymClass.EndsAt = request.EndsAt;
        gymClass.Location = NormalizeOptional(request.Location);
        gymClass.Capacity = request.Capacity;
        gymClass.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        var response = await BuildResponseAsync(gymClass.Id, tenantId, db);
        return Results.Ok(response);
    }

    /// <summary>
    /// Remove uma aula do tenant atual.
    /// Neste primeiro recorte, a exclusão é física.
    /// </summary>
    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, HttpContext context)
    {
        if (!CanManageClasses(context.User))
            return Results.Forbid();

        var tenantId = context.GetTenantId();
        var gymClass = await db.Set<GymClass>().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (gymClass is null)
            return Results.NotFound();

        db.Set<GymClass>().Remove(gymClass);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Cria uma reserva para o usuário autenticado em uma aula do tenant atual.
    /// </summary>
    private static async Task<IResult> BookAsync(Guid id, AppDbContext db, HttpContext context)
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
    /// Remove a reserva do usuário autenticado para uma aula do tenant atual.
    /// </summary>
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
    /// Recarrega a aula após escrita para devolver a mesma projeção usada nas consultas.
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
                                    gc.created_at AS CreatedAt
                             FROM gym_classes gc
                             LEFT JOIN users u ON u.id = gc.instructor_user_id AND u.tenant_id = @TenantId
                             WHERE gc.id = @Id AND gc.tenant_id = @TenantId";

        return (await connection.QueryFirstAsync<GymClassResponse>(sql, new { Id = id, TenantId = tenantId }))!;
    }

    /// <summary>
    /// Define quais perfis podem gerenciar o cadastro de aulas.
    /// </summary>
    private static bool CanManageClasses(ClaimsPrincipal user)
    {
        var role = user.FindFirstValue(ClaimTypes.Role);
        return role is nameof(UserRole.Admin) or nameof(UserRole.Teacher);
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

    /// <summary>
    /// Normaliza campos opcionais para evitar persistência de espaços em branco.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
