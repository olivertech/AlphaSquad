namespace AlphaSquad.Api.Features.Media;

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
