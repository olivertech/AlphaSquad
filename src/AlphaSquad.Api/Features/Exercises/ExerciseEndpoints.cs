namespace AlphaSquad.Api.Features.Exercises;

public static class ExerciseEndpoints
{
    public static IEndpointRouteBuilder MapExerciseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exercises")
            .WithTags("Exercises")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetExercises")
            .Produces<List<ExerciseResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetExerciseById")
            .Produces<ExerciseResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateExercise")
            .Produces<ExerciseResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateExercise")
            .Produces<ExerciseResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteExercise")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

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
                             LEFT JOIN tenant_medias m ON e.media_id = m.id 
                             WHERE e.tenant_id = @TenantId 
                             ORDER BY e.name";

        var exercises = await connection.QueryAsync<ExerciseResponse>(sql, new { TenantId = tenantId });

        return Results.Ok(exercises.ToList());
    }

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
                             LEFT JOIN tenant_medias m ON e.media_id = m.id 
                             WHERE e.id = @Id AND e.tenant_id = @TenantId";

        var exercise = await connection.QueryFirstOrDefaultAsync<ExerciseResponse>(sql, new { Id = id, TenantId = tenantId });

        if (exercise is null)
            return Results.NotFound();

        return Results.Ok(exercise);
    }

    private static async Task<IResult> CreateAsync(ExerciseCreateRequest request, AppDbContext db, HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(request.MuscleGroup))
            return Results.BadRequest("Muscle group is required.");

        var tenantId = context.GetTenantId();

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
