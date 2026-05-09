namespace AlphaSquad.Api.Features.Workouts;

public static class WorkoutEndpoints
{
    /// <summary>
    /// Registra os endpoints do modulo de treinos.
    /// As consultas ficam disponiveis para qualquer usuario autenticado do tenant,
    /// enquanto as operacoes de escrita ficam restritas a administradores.
    /// </summary>
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
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateWorkout")
            .WithSummary("Cria um novo treino no tenant atual.")
            .WithDescription("Cadastra um treino com nome, descrição e objetivo para uso dentro do tenant da sessão.")
            .Produces<WorkoutResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateWorkout")
            .WithSummary("Atualiza um treino do tenant atual.")
            .WithDescription("Permite alterar nome, objetivo, descrição e status ativo de um treino existente.")
            .Produces<WorkoutResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteWorkout")
            .WithSummary("Remove um treino do tenant atual.")
            .WithDescription("Exclui um treino pelo identificador, respeitando o isolamento do tenant autenticado.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/exercises", AssignExercisesAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("AssignWorkoutExercises")
            .WithSummary("Define os exercícios de um treino.")
            .WithDescription("Substitui a composição atual do treino por uma nova lista ordenada de exercícios do mesmo tenant.")
            .Produces<WorkoutDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Lista os treinos cadastrados para o tenant autenticado.
    /// Essa visao resumida apoia selecao e navegacao antes do detalhamento do treino.
    /// </summary>
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

    /// <summary>
    /// Retorna o detalhamento de um treino especifico com a composicao ordenada de exercicios.
    /// A consulta reforca o filtro por tenant tanto no treino quanto nos exercicios associados.
    /// </summary>
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
                                      LEFT JOIN tenant_medias m ON e.media_id = m.id AND m.tenant_id = @TenantId
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

    /// <summary>
    /// Cria um novo treino no tenant atual.
    /// O nome precisa ser valido e unico dentro da academia para facilitar a operacao do time.
    /// </summary>
    private static async Task<IResult> CreateAsync(WorkoutCreateRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var validation = await ValidateWorkoutRequestAsync(request.Name, tenantId, db);
        if (validation is not null)
            return validation;

        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            Goal = NormalizeOptional(request.Goal),
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

    /// <summary>
    /// Atualiza os dados principais de um treino existente.
    /// A mesma validacao de nome usada na criacao tambem vale na edicao.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id, WorkoutUpdateRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var workout = await db.Workouts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (workout is null)
            return Results.NotFound();

        var validation = await ValidateWorkoutRequestAsync(request.Name, tenantId, db, id);
        if (validation is not null)
            return validation;

        workout.Name = request.Name.Trim();
        workout.Description = NormalizeOptional(request.Description);
        workout.Goal = NormalizeOptional(request.Goal);
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

    /// <summary>
    /// Remove um treino do tenant atual.
    /// Nesta V1 a exclusao continua fisica, incluindo a composicao associada por cascade do banco.
    /// </summary>
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

    /// <summary>
    /// Substitui a composicao de exercicios de um treino.
    /// O payload pode representar tanto uma lista nova quanto a limpeza completa da composicao atual.
    /// </summary>
    private static async Task<IResult> AssignExercisesAsync(Guid id, [FromBody] List<ExerciseAssignmentRequest> requests, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var workout = await db.Workouts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (workout is null)
            return Results.NotFound();

        if (requests == null)
            return Results.BadRequest("Exercise list payload is required.");

        var assignmentsValidation = ValidateAssignments(requests);
        if (assignmentsValidation is not null)
            return assignmentsValidation;

        var requestedExerciseIds = requests
            .Select(x => x.ExerciseId)
            .Distinct()
            .ToList();

        // A composicao antiga e removida primeiro para que o payload represente sempre o estado final desejado.
        var existingAssocs = await db.WorkoutExercises.Where(x => x.WorkoutId == id).ToListAsync();
        db.WorkoutExercises.RemoveRange(existingAssocs);

        if (requestedExerciseIds.Count > 0)
        {
            var validExercisesCount = await db.Exercises
                .CountAsync(x => requestedExerciseIds.Contains(x.Id) && x.TenantId == tenantId);

            if (validExercisesCount != requestedExerciseIds.Count)
                return Results.BadRequest("One or more exercises do not belong to this tenant.");
        }

        foreach (var req in requests)
        {
            db.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = id,
                ExerciseId = req.ExerciseId,
                Order = req.Order,
                Sets = req.Sets,
                Reps = req.Reps.Trim(),
                RestTime = req.RestTime,
                Notes = NormalizeOptional(req.Notes)
            });
        }

        await db.SaveChangesAsync();

        // Recarrega a resposta via Dapper para manter o mesmo formato usado nas leituras do modulo.
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
                                      LEFT JOIN tenant_medias m ON e.media_id = m.id AND m.tenant_id = @TenantId
                                      WHERE we.workout_id = @WorkoutId
                                        AND w.tenant_id = @TenantId
                                        AND e.tenant_id = @TenantId
                                      ORDER BY we.""order\""";

        var workoutRes = await connection.QueryFirstOrDefaultAsync<WorkoutResponse>(workoutSql, new { Id = id, TenantId = tenantId });
        var exercisesRes = await connection.QueryAsync<WorkoutExerciseResponse>(exercisesSql, new { WorkoutId = id, TenantId = tenantId });

        return Results.Ok(new WorkoutDetailsResponse(workoutRes!, exercisesRes.ToList()));
    }

    /// <summary>
    /// Valida o payload principal do treino antes da persistencia.
    /// </summary>
    private static async Task<IResult?> ValidateWorkoutRequestAsync(string name, Guid tenantId, AppDbContext db, Guid? workoutIdToIgnore = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Workout name is required.");

        var normalizedName = name.Trim();

        if (normalizedName.Length < 3)
            return Results.BadRequest("Workout name must have at least 3 characters.");

        if (normalizedName.Length > 150)
            return Results.BadRequest("Workout name must have at most 150 characters.");

        var duplicatedNameExists = await db.Workouts.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.Id != workoutIdToIgnore &&
            x.Name.ToLower() == normalizedName.ToLower());

        if (duplicatedNameExists)
            return Results.Conflict("A workout with this name already exists in this tenant.");

        return null;
    }

    /// <summary>
    /// Valida a composicao enviada para o treino.
    /// Isso ajuda a impedir inconsistencias simples antes de gravar a associacao.
    /// </summary>
    private static IResult? ValidateAssignments(List<ExerciseAssignmentRequest> requests)
    {
        // Lista vazia e permitida para suportar a limpeza completa da composicao do treino.
        if (requests.Count == 0)
            return null;

        if (requests.Select(x => x.ExerciseId).Distinct().Count() != requests.Count)
            return Results.BadRequest("Exercise list cannot contain duplicated exercises.");

        if (requests.Select(x => x.Order).Distinct().Count() != requests.Count)
            return Results.BadRequest("Exercise list cannot contain duplicated order values.");

        foreach (var request in requests)
        {
            if (request.Order <= 0)
                return Results.BadRequest("Exercise order must be greater than zero.");

            if (request.Sets <= 0)
                return Results.BadRequest("Exercise sets must be greater than zero.");

            if (string.IsNullOrWhiteSpace(request.Reps))
                return Results.BadRequest("Exercise reps are required.");

            if (request.RestTime < 0)
                return Results.BadRequest("Exercise rest time cannot be negative.");
        }

        return null;
    }

    /// <summary>
    /// Normaliza campos opcionais para evitar persistencia de textos vazios.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
