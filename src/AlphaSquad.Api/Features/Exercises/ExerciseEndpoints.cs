using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using AlphaSquad.Infrastructure.Persistence;
using AlphaSquad.Shared.DTOs.Exercises;

namespace AlphaSquad.Api.Features.Exercises;

/// <summary>
/// Define os endpoints para o gerenciamento de Exercícios.
/// Segue o padrão de Vertical Slice, concentrando a lógica de acesso aos dados e regras de negócio para esta feature.
/// </summary>
public static class ExerciseEndpoints
{
    /// <summary>
    /// Mapeia as rotas de Exercícios no sistema.
    /// </summary>
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

    /// <summary>
    /// Recupera todos os exercícios vinculados ao Tenant autenticado.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var exercises = await db.Exercises
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .Select(x => new ExerciseResponse(
                x.Id,
                x.Name,
                x.MuscleGroup,
                x.Description,
                x.MediaId,
                x.Media != null ? x.Media.Url : null,
                x.CreatedAt
            ))
            .ToListAsync();

        return Results.Ok(exercises);
    }

    /// <summary>
    /// Recupera um exercício específico por ID, garantindo que pertença ao Tenant autenticado.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var exercise = await db.Exercises
            .AsNoTracking()
            .Include(x => x.Media)
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(x => new ExerciseResponse(
                x.Id,
                x.Name,
                x.MuscleGroup,
                x.Description,
                x.MediaId,
                x.Media != null ? x.Media.Url : null,
                x.CreatedAt
            ))
            .FirstOrDefaultAsync();

        if (exercise is null)
            return Results.NotFound();

        return Results.Ok(exercise);
    }

    /// <summary>
    /// Cria um novo exercício para a academia autenticada.
    /// </summary>
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
            null, // A URL da mídia seria resolvida em uma consulta separada ou via Join
            exercise.CreatedAt
        );

        return Results.Created($"/api/exercises/{exercise.Id}", response);
    }

    /// <summary>
    /// Atualiza os dados de um exercício existente, validando a propriedade do Tenant.
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
    /// Remove permanentemente um exercício do sistema.
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
