namespace AlphaSquad.Api.Features.Media;

/// <summary>
/// O MapMediaEndpoints define os endpoints relacionados à mídia, como upload de arquivos. 
/// Ele é usado para organizar e agrupar as rotas de mídia sob um prefixo 
/// comum (/api/media) e aplicar tags para documentação.
/// Com essa classe e método, o código de configuração dos endpoints de mídia 
/// fica centralizado e fácil de manter, além de melhorar a clareza e a organização do código da API.
/// No Program.cs, o método MapMediaEndpoints é chamado para registrar esses endpoints na aplicação, 
/// ao invés de chamar MapControllers, garantindo que as rotas de mídia estejam disponíveis para os clientes da API.
/// </summary>
/// <param name="app"></param>
/// <returns></returns>
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

        // Atulizar a midia (ex: renomear o arquivo)
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

        group.MapGet("/", GetAllAsync)
            .RequireAuthorization()
            .WithName("GetTenantMedias")
            .Produces<List<TenantMediaResponse>>(StatusCodes.Status200OK);

        return app;
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

        // 1 - Buscar a mídia no banco de dados, garantindo que ela pertence ao tenant
        var media = await db.TenantMedias.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (media is null)
            return Results.NotFound();

        var oldStorageKey = media.StorageKey;

        var path = $"tenants/{tenantSlug}/media";

        using var stream = file.OpenReadStream();

        // 2 - Fazer o upload do novo arquivo para o storage, obtendo a nova URL e StorageKey
        var uploadResult = await storage.UploadAsync(stream,
                                                     file.FileName,
                                                     file.ContentType,
                                                     path);

        media.FileName = file.FileName;
        media.ContentType = file.ContentType;
        media.Size = file.Length;
        media.StorageKey = uploadResult.Key;
        media.Url = uploadResult.Url;

        // 3 - Atualizar os dados da mídia no banco de dados
        await db.SaveChangesAsync();

        // 4 - Deletar o arquivo antigo do storage
        await storage.DeleteAsync(oldStorageKey);

        // 5 - Retornar os dados atualizados da mídia
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
        // 1. Validação do arquivo
        if (file == null || file.Length == 0)
            return Results.BadRequest("File is required.");

        // 2. Recupera os dados do Tenant vindo do JWT
        var tenantId = context.GetTenantId();
        var tenantSlug = context.GetTenantSlug();

        // 3. Caminho lógico no storage
        var path = $"tenants/{tenantSlug}/media";

        // 4. Upload
        using var stream = file.OpenReadStream();

        var result = await storage.UploadAsync(
            stream,
            file.FileName.ToLower(),
            file.ContentType,
            path
        );

        // 5. Persistência no banco
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

        // 6. Retorno
        return Results.Ok(new
        {
            media.Id,
            media.Url,
            media.FileName
        });
    }
}
