using Microsoft.AspNetCore.Http.HttpResults;

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

        return app;
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
            FileName = file.FileName,
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
