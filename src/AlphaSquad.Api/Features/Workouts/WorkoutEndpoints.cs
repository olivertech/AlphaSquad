namespace AlphaSquad.Api.Features.Workouts;

public static class WorkoutEndpoints
{
    public static IEndpointRouteBuilder MapWorkoutEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workouts")
            .WithTags("Workouts")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetWorkouts")
            .WithSummary("Lista os treinos do tenant atual.")
            .WithDescription("Retorna os treinos cadastrados para o tenant autenticado.")
            .Produces<List<WorkoutResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetWorkoutById")
            .WithSummary("Busca um treino específico do tenant atual.")
            .WithDescription("Retorna o treino e a lista ordenada de exercícios associados a ele.")
            .Produces<WorkoutDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateWorkout")
            .WithSummary("Cria um novo treino no tenant atual.")
            .WithDescription("Cadastra um treino com nome, descrição e objetivo para uso dentro do tenant da sessão.")
            .Produces<WorkoutResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateWorkout")
            .WithSummary("Atualiza um treino do tenant atual.")
            .WithDescription("Permite alterar nome, objetivo, descrição e status ativo de um treino existente.")
            .Produces<WorkoutResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteWorkout")
            .WithSummary("Remove um treino do tenant atual.")
            .WithDescription("Exclui um treino pelo identificador, respeitando o isolamento do tenant autenticado.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/exercises", AssignExercisesAsync)
            .WithName("AssignWorkoutExercises")
            .WithSummary("Define os exercícios de um treino.")
            .WithDescription("Substitui a composição atual do treino por uma nova lista ordenada de exercícios do mesmo tenant.")
            .Produces<WorkoutDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAllAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT id, name, description, goal, is_active AS IsActive, created_at AS CreatedAt 
                             FROM workouts 
                             WHERE tenant_id = @TenantId 
                             ORDER BY name";

        var workouts = await connection.QueryAsync<WorkoutResponse>(sql, new { TenantId = tenantId });

        return Results.Ok(workouts);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string workoutSql = @"SELECT id, name, description, goal, is_active AS IsActive, created_at AS CreatedAt 
                                    FROM workouts 
                                    WHERE id = @Id AND tenant_id = @TenantId";

        const string exercisesSql = @"SELECT we.exercise_id AS ExerciseId, e.name AS ExerciseName, m.url AS MediaUrl, 
                                      we.""order"", we.sets, we.reps, we.rest_time AS RestTime, we.notes 
                                      FROM workout_exercises we
                                      JOIN workouts w ON w.id = we.workout_id
                                      JOIN exercises e ON we.exercise_id = e.id
                                      LEFT JOIN tenant_medias m ON e.media_id = m.id
                                      WHERE we.workout_id = @WorkoutId
                                        AND w.tenant_id = @TenantId
                                        AND e.tenant_id = @TenantId
                                      ORDER BY we.""order\""";

        var workout = await connection.QueryFirstOrDefaultAsync<WorkoutResponse>(workoutSql, new { Id = id, TenantId = tenantId });

        if (workout is null)
            return Results.NotFound();

        var exercises = await connection.QueryAsync<WorkoutExerciseResponse>(exercisesSql, new { WorkoutId = id, TenantId = tenantId });

        return Results.Ok(new WorkoutDetailsResponse(workout, exercises.ToList()));
    }

    private static async Task<IResult> CreateAsync(WorkoutCreateRequest request, AppDbContext db, HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Workout name is required.");

        var tenantId = context.GetTenantId();

        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = request.Description,
            Goal = request.Goal,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Workouts.Add(workout);
        await db.SaveChangesAsync();

        return Results.Created($"/api/workouts/{workout.Id}", new WorkoutResponse(
            workout.Id,
            workout.Name,
            workout.Description,
            workout.Goal,
            workout.IsActive,
            workout.CreatedAt
        ));
    }

    private static async Task<IResult> UpdateAsync(Guid id, WorkoutUpdateRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var workout = await db.Workouts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (workout is null)
            return Results.NotFound();

        workout.Name = request.Name.Trim();
        workout.Description = request.Description;
        workout.Goal = request.Goal;
        workout.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return Results.Ok(new WorkoutResponse(
            workout.Id,
            workout.Name,
            workout.Description,
            workout.Goal,
            workout.IsActive,
            workout.CreatedAt
        ));
    }

    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var workout = await db.Workouts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (workout is null)
            return Results.NotFound();

        db.Workouts.Remove(workout);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> AssignExercisesAsync(Guid id, [FromBody] List<ExerciseAssignmentRequest> requests, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var workout = await db.Workouts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (workout is null)
            return Results.NotFound();

        if (requests == null || requests.Count == 0)
            return Results.BadRequest("Exercise list is required.");

        // Remove current associations to replace with new set
        var existingAssocs = await db.WorkoutExercises.Where(x => x.WorkoutId == id).ToListAsync();
        db.WorkoutExercises.RemoveRange(existingAssocs);

        foreach (var req in requests)
        {
            // Validar se o exercício pertence ao tenant
            var exerciseExists = await db.Exercises.AnyAsync(x => x.Id == req.ExerciseId && x.TenantId == tenantId);
            if (!exerciseExists)
                return Results.BadRequest($"Exercise {req.ExerciseId} does not belong to this tenant.");

            db.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = id,
                ExerciseId = req.ExerciseId,
                Order = req.Order,
                Sets = req.Sets,
                Reps = req.Reps,
                RestTime = req.RestTime,
                Notes = req.Notes
            });
        }

        await db.SaveChangesAsync();

        // Retornar detalhes atualizados via Dapper para consistência
        var connection = db.Database.GetDbConnection();
        const string workoutSql = @"SELECT id, 
                                           name, 
                                           description, 
                                           goal, 
                                           is_active AS IsActive, 
                                           created_at AS CreatedAt 
                                    FROM workouts 
                                    WHERE id = @Id
                                      AND tenant_id = @TenantId";

        const string exercisesSql = @"SELECT we.exercise_id AS ExerciseId, 
                                             e.name AS ExerciseName, 
                                             m.url AS MediaUrl, 
                                             we.""order"", 
                                             we.sets, 
                                             we.reps, 
                                             we.rest_time AS RestTime, 
                                             we.notes 
                                      FROM workout_exercises we
                                      JOIN workouts w ON w.id = we.workout_id
                                      JOIN exercises e ON we.exercise_id = e.id
                                      LEFT JOIN tenant_medias m ON e.media_id = m.id
                                      WHERE we.workout_id = @WorkoutId
                                        AND w.tenant_id = @TenantId
                                        AND e.tenant_id = @TenantId
                                      ORDER BY we.""order\""";

        var workoutRes = await connection.QueryFirstOrDefaultAsync<WorkoutResponse>(workoutSql, new { Id = id, TenantId = tenantId });
        var exercisesRes = await connection.QueryAsync<WorkoutExerciseResponse>(exercisesSql, new { WorkoutId = id, TenantId = tenantId });

        return Results.Ok(new WorkoutDetailsResponse(workoutRes!, exercisesRes.ToList()));
    }
}
