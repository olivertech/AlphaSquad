namespace AlphaSquad.Api.Features.Media;

public static class MediaEndpoints
{
    /// <summary>
    /// Registra os endpoints do modulo de midias.
    /// Todo acesso e restrito a perfis de gestao porque os arquivos servem de apoio aos demais modulos do tenant.
    /// </summary>
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media")
            .WithTags("Media")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapPost("/upload", UploadAsync)
            .DisableAntiforgery()
            .WithName("UploadMedia")
            .WithSummary("Faz upload de uma mídia para o tenant atual.")
            .WithDescription("Armazena o arquivo no storage configurado e cria o registro da mídia vinculado ao tenant da sessão.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UploadMediaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/file", ReplaceFileAsync)
            .DisableAntiforgery()
            .WithName("ReplaceMediaFile")
            .WithSummary("Substitui o arquivo de uma mídia existente.")
            .WithDescription("Envia um novo arquivo para o storage, atualiza os metadados da mídia e remove o binário anterior.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<TenantMediaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteMedia")
            .WithSummary("Remove uma mídia do tenant atual.")
            .WithDescription("Exclui o arquivo no storage e remove o registro da mídia, respeitando vínculos ativos com o tenant.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetAllAsync)
            .WithName("GetTenantMedias")
            .WithSummary("Lista as mídias do tenant atual.")
            .WithDescription("Retorna uma lista paginada das mídias cadastradas para o tenant autenticado.")
            .Produces<PagedResponse<TenantMediaResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetMediaById")
            .WithSummary("Busca uma mídia específica do tenant atual.")
            .WithDescription("Retorna os dados de uma mídia pelo identificador, desde que ela pertença ao tenant da sessão.")
            .Produces<MediaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Retorna os dados publicos de uma midia especifica do tenant atual.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT id, file_name AS FileName, content_type AS ContentType, url 
                             FROM tenant_medias 
                             WHERE id = @Id AND tenant_id = @TenantId";

        var media = await connection.QueryFirstOrDefaultAsync<MediaResponse>(sql, new { Id = id, TenantId = tenantId });

        if (media is null)
            return Results.NotFound();

        return Results.Ok(media);
    }

    /// <summary>
    /// Lista as midias do tenant autenticado com paginacao simples.
    /// Essa consulta serve como base de selecao para logo, exercicios, eventos e loja.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db,
                                                   HttpContext context,
                                                   int page = 1,
                                                   int pageSize = 20)
    {
        var tenantId = context.GetTenantId();

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var connection = db.Database.GetDbConnection();

        const string countSql = @"SELECT COUNT(*) FROM tenant_medias WHERE tenant_id = @TenantId";
        const string itemsSql = @"SELECT id, tenant_id AS TenantId, file_name AS FileName, content_type AS ContentType, 
                                  size, storage_key AS StorageKey, url, created_at AS CreatedAt 
                                  FROM tenant_medias 
                                  WHERE tenant_id = @TenantId 
                                  ORDER BY created_at DESC 
                                  LIMIT @Limit OFFSET @Offset";

        var total = await connection.ExecuteScalarAsync<int>(countSql, new { TenantId = tenantId });
        var items = await connection.QueryAsync<TenantMediaResponse>(itemsSql, new 
        { 
            TenantId = tenantId, 
            Limit = pageSize, 
            Offset = (page - 1) * pageSize 
        });

        return Results.Ok(new PagedResponse<TenantMediaResponse>(page, pageSize, total, items.ToList()));
    }

    /// <summary>
    /// Substitui o binario de uma midia existente preservando o mesmo registro logico.
    /// O arquivo anterior so e removido depois que o novo upload foi concluido.
    /// </summary>
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
        var uploadResult = await storage.UploadAsync(stream, file.FileName, file.ContentType, path);

        media.FileName = file.FileName;
        media.ContentType = file.ContentType;
        media.Size = file.Length;
        media.StorageKey = uploadResult.Key;
        media.Url = uploadResult.Url;

        await db.SaveChangesAsync();
        await storage.DeleteAsync(oldStorageKey);

        return Results.Ok(new TenantMediaResponse(
            media.Id,
            media.TenantId,
            media.FileName.ToLower(),
            media.ContentType,
            media.Size,
            media.Url,
            media.CreatedAt
        ));
    }

    /// <summary>
    /// Remove uma midia do tenant atual quando ela nao esta mais em uso.
    /// A exclusao trata primeiro referencias sensiveis, como logo do tenant e exercicios vinculados.
    /// </summary>
    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, IObjectStorageService storage, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var media = await db.TenantMedias
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (media is null)
            return Results.NotFound();

        // Se a midia estiver em uso como logo do tenant atual, o vinculo e limpo antes da remocao fisica.
        var tenantWithLogo = await db.Tenants.FirstOrDefaultAsync(x => x.LogoMediaId == id && x.Id == tenantId);
        if (tenantWithLogo != null)
        {
            tenantWithLogo.LogoMediaId = null;
            tenantWithLogo.LogoUrl = null;
            await db.SaveChangesAsync();
        }

        var mediaInUseByExercise = await db.Exercises.AnyAsync(x => x.MediaId == id && x.TenantId == tenantId);
        if (mediaInUseByExercise)
            return Results.BadRequest("Media is in use by an exercise and cannot be deleted.");

        await storage.DeleteAsync(media.StorageKey);

        db.TenantMedias.Remove(media);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Faz upload de uma nova midia para o tenant atual e persiste seus metadados basicos.
    /// </summary>
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
