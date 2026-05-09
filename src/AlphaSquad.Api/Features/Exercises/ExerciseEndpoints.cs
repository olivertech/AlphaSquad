namespace AlphaSquad.Api.Features.Exercises;

public static class ExerciseEndpoints
{
    /// <summary>
    /// Registra os endpoints do modulo de exercicios.
    /// Leituras ficam disponiveis para autenticados; escritas exigem perfis de gestao.
    /// </summary>
    public static IEndpointRouteBuilder MapExerciseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exercises")
            .WithTags("Exercises")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetExercises")
            .WithSummary("Lista os exercícios do tenant atual.")
            .WithDescription("Retorna os exercícios cadastrados para o tenant, incluindo mídia associada quando existir.")
            .Produces<List<ExerciseResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetExerciseById")
            .WithSummary("Busca um exercício específico do tenant atual.")
            .WithDescription("Retorna o detalhamento de um exercício pelo identificador, respeitando o tenant da sessão.")
            .Produces<ExerciseResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateExercise")
            .WithSummary("Cria um novo exercício no tenant atual.")
            .WithDescription("Cadastra um exercício com grupo muscular, descrição e mídia opcional pertencente ao mesmo tenant.")
            .Produces<ExerciseResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateExercise")
            .WithSummary("Atualiza um exercício do tenant atual.")
            .WithDescription("Permite alterar os dados de um exercício já existente, incluindo a mídia opcional associada.")
            .Produces<ExerciseResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteExercise")
            .WithSummary("Remove um exercício do tenant atual.")
            .WithDescription("Exclui um exercício pelo identificador, desde que ele pertença ao tenant da sessão.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Lista os exercicios do tenant atual, incluindo a URL da midia quando existir.
    /// O join com midia tambem respeita o tenant para evitar cruzamento indevido de dados.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT e.id, 
                                    e.name, 
                                    e.muscle_group, 
                                    e.description, 
                                    e.media_id, 
                                    m.url, 
                                    e.created_at 
                             FROM exercises e 
                             LEFT JOIN tenant_medias m ON e.media_id = m.id AND m.tenant_id = @TenantId
                             WHERE e.tenant_id = @TenantId 
                             ORDER BY e.name";

        var exercises = await connection.QueryAsync<ExerciseResponse>(sql, new { TenantId = tenantId });

        return Results.Ok(exercises.ToList());
    }

    /// <summary>
    /// Retorna o detalhamento de um exercicio especifico do tenant atual.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT e.id, 
                                    e.name, 
                                    e.muscle_group, 
                                    e.description, 
                                    e.media_id, 
                                    m.url, 
                                    e.created_at 
                             FROM exercises e 
                             LEFT JOIN tenant_medias m ON e.media_id = m.id AND m.tenant_id = @TenantId
                             WHERE e.id = @Id AND e.tenant_id = @TenantId";

        var exercise = await connection.QueryFirstOrDefaultAsync<ExerciseResponse>(sql, new { Id = id, TenantId = tenantId });

        if (exercise is null)
            return Results.NotFound();

        return Results.Ok(exercise);
    }

    /// <summary>
    /// Cria um novo exercicio para o tenant autenticado.
    /// Quando houver `MediaId`, a midia precisa pertencer ao mesmo tenant do exercicio.
    /// </summary>
    private static async Task<IResult> CreateAsync(ExerciseCreateRequest request, AppDbContext db, HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(request.MuscleGroup))
            return Results.BadRequest("Muscle group is required.");

        var tenantId = context.GetTenantId();

        if (request.MediaId.HasValue)
        {
            var mediaExists = await db.TenantMedias.AnyAsync(x => x.Id == request.MediaId.Value && x.TenantId == tenantId);
            if (!mediaExists)
                return Results.BadRequest("Media does not belong to this tenant.");
        }

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            MuscleGroup = request.MuscleGroup.Trim(),
            Description = request.Description,
            MediaId = request.MediaId,
            CreatedAt = DateTime.UtcNow
        };

        db.Exercises.Add(exercise);
        await db.SaveChangesAsync();

        var response = new ExerciseResponse(
            exercise.Id,
            exercise.Name,
            exercise.MuscleGroup,
            exercise.Description,
            exercise.MediaId,
            null, 
            exercise.CreatedAt
        );

        return Results.Created($"/api/exercises/{exercise.Id}", response);
    }

    /// <summary>
    /// Atualiza um exercicio existente do tenant atual.
    /// Mantem a mesma validacao de pertencimento de midia aplicada na criacao.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id, ExerciseUpdateRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var exercise = await db.Exercises.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (exercise is null)
            return Results.NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(request.MuscleGroup))
            return Results.BadRequest("Muscle group is required.");

        if (request.MediaId.HasValue)
        {
            var mediaExists = await db.TenantMedias.AnyAsync(x => x.Id == request.MediaId.Value && x.TenantId == tenantId);
            if (!mediaExists)
                return Results.BadRequest("Media does not belong to this tenant.");
        }

        exercise.Name = request.Name.Trim();
        exercise.MuscleGroup = request.MuscleGroup.Trim();
        exercise.Description = request.Description;
        exercise.MediaId = request.MediaId;

        await db.SaveChangesAsync();

        var response = new ExerciseResponse(
            exercise.Id,
            exercise.Name,
            exercise.MuscleGroup,
            exercise.Description,
            exercise.MediaId,
            null,
            exercise.CreatedAt
        );

        return Results.Ok(response);
    }

    /// <summary>
    /// Remove um exercicio do tenant atual.
    /// Nesta V1 a exclusao continua fisica.
    /// </summary>
    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var exercise = await db.Exercises.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (exercise is null)
            return Results.NotFound();

        db.Exercises.Remove(exercise);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
