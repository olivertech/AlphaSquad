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

    private static async Task<IResult> UploadAsync(IFormFile file, IObjectStorageService storage)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("File is required.");

        var path = "tenants/demo/uploads"; // TODO: evoluir isso depois para TenantId real

        using var stream = file.OpenReadStream();

        var url = await storage.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            path
        );

        return Results.Ok(new UploadMediaResponse
        {
            Url = url
        });
    }
}
