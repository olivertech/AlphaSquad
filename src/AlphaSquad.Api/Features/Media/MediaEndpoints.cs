namespace AlphaSquad.Api.Features.Media;

using AlphaSquad.Infrastructure.Persistence;
using AlphaSquad.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// O MapMediaEndpoints define os endpoints relacionados à mídia, como upload de arquivos. 
/// Ele é usado para organizar e agrupar as rotas de mídia sob um prefixo 
/// comum (/api/media) e aplicar tags para documentação.
/// </summary>
public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media")
            .WithTags("Media");

        // Upload de arquivo (imagem, logo, etc)
        group.MapPost("/upload", UploadAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UploadMedia")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UploadMediaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        // Atualizar a midia (ex: renomear o arquivo)
        group.MapPut("/{id:guid}/file", ReplaceFileAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("ReplaceMediaFile")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<TenantMediaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        // Deletar a mídia
        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization()
            .WithName("DeleteMedia")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // Listar mídias do tenant com paginação
        group.MapGet("/", GetAllAsync)
            .RequireAuthorization()
            .WithName("GetTenantMedias")
            .Produces<List<TenantMediaResponse>>(StatusCodes.Status200OK);

        // Obter detalhes de uma mídia específica
        group.MapGet("/{id:guid}", GetByIdAsync)
            .RequireAuthorization()
            .WithName("GetMediaById")
            .Produces<MediaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Obtém os detalhes de um arquivo de mídia específico, garantindo que ele pertença ao tenant do usuário.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var media = await db.TenantMedias
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(x => new MediaResponse
            {
                Id = x.Id,
                FileName = x.FileName,
                ContentType = x.ContentType,
                Url = x.Url
            })
            .FirstOrDefaultAsync();

        if (media is null)
            return Results.NotFound();

        return Results.Ok(media);
    }

    private static async Task<IResult> GetAllAsync(AppDbContext db,
                                                   HttpContext context,
                                                   int page = 1,
                                                   int pageSize = 20)
    {
        var tenantId = context.GetTenantId();

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = db.TenantMedias
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TenantMediaResponse
            {
                Id = x.Id,
                TenantId = x.TenantId,
                FileName = x.FileName,
                ContentType = x.ContentType,
                Size = x.Size,
                StorageKey = x.StorageKey,
                Url = x.Url,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Results.Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }

    private static async Task<IResult> ReplaceFileAsync(Guid id, 
                                                        IFormFile file, 
                                                        AppDbContext db,
                                                        IObjectStorageService storage, 
                                                        HttpContext context)
    {
        if (file is null || file.Length == 0)
            return Results.BadRequest("File is required.");

        var tenantId = context.GetTenantId();
        var tenantSlug = context.GetTenantSlug();

        var media = await db.TenantMedias.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (media is null)
            return Results.NotFound();

        var oldStorageKey = media.StorageKey;

        var path = $"tenants/{tenantSlug}/media";

        using var stream = file.OpenReadStream();

        var uploadResult = await storage.UploadAsync(stream,
                                                     file.FileName,
                                                     file.ContentType,
                                                     path);

        media.FileName = file.FileName;
        media.ContentType = file.ContentType;
        media.Size = file.Length;
        media.StorageKey = uploadResult.Key;
        media.Url = uploadResult.Url;

        await db.SaveChangesAsync();

        await storage.DeleteAsync(oldStorageKey);

        return Results.Ok(new TenantMediaResponse
        {
            Id = media.Id,
            TenantId = media.TenantId,
            FileName = media.FileName.ToLower(),
            ContentType = media.ContentType,
            Size = media.Size,
            StorageKey = media.StorageKey,
            Url = media.Url,
            CreatedAt = media.CreatedAt
        });
    }

    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, IObjectStorageService storage, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var media = await db.TenantMedias
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (media is null)
            return Results.NotFound();

        await storage.DeleteAsync(media.StorageKey);

        db.TenantMedias.Remove(media);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> UploadAsync(IFormFile file, AppDbContext db, IObjectStorageService storage, HttpContext context)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("File is required.");

        var tenantId = context.GetTenantId();
        var tenantSlug = context.GetTenantSlug();

        var path = $"tenants/{tenantSlug}/media";

        using var stream = file.OpenReadStream();

        var result = await storage.UploadAsync(
            stream,
            file.FileName.ToLower(),
            file.ContentType,
            path
        );

        var media = new TenantMedia
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FileName = file.FileName.ToLower(),
            ContentType = file.ContentType,
            Size = file.Length,
            StorageKey = result.Key,
            Url = result.Url,
            CreatedAt = DateTime.UtcNow
        };

        db.TenantMedias.Add(media);
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            media.Id,
            media.Url,
            media.FileName
        });
    }
}
