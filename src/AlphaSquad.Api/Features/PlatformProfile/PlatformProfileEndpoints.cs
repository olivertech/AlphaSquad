namespace AlphaSquad.Api.Features.PlatformProfile;

/// <summary>
/// Registra os endpoints de perfil do owner do backoffice master.
/// O objetivo e espelhar a experiencia de profile do dashboard das academias, mas no contexto global da plataforma.
/// </summary>
public static class PlatformProfileEndpoints
{
    public static IEndpointRouteBuilder MapPlatformProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/platform-profile")
            .WithTags("Platform Profile")
            .RequireAuthorization(AuthorizationPolicies.PlatformOwnerOnly);

        group.MapGet("/me", GetMeAsync)
            .WithName("GetPlatformProfile")
            .WithSummary("Retorna o perfil do owner autenticado.")
            .Produces<PlatformProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/me", UpdateAsync)
            .WithName("UpdatePlatformProfile")
            .WithSummary("Atualiza os dados basicos do owner autenticado.")
            .Produces<PlatformProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/me/photo", UpdatePhotoAsync)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("UpdatePlatformProfilePhoto")
            .WithSummary("Atualiza a foto de perfil do owner autenticado.")
            .Produces<PlatformProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/me/photo", DeletePhotoAsync)
            .WithName("DeletePlatformProfilePhoto")
            .WithSummary("Remove a foto de perfil do owner autenticado.")
            .Produces<PlatformProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> GetMeAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var platformUser = await ResolvePlatformUserAsync(db, user);
        return platformUser is null ? Results.Unauthorized() : Results.Ok(Map(platformUser));
    }

    private static async Task<IResult> UpdateAsync([FromBody] UpdatePlatformProfileRequest request,
                                                   AppDbContext db,
                                                   ClaimsPrincipal user)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        var platformUser = await ResolvePlatformUserAsync(db, user);
        if (platformUser is null)
            return Results.Unauthorized();

        platformUser.Name = request.Name.Trim();
        await db.SaveChangesAsync();

        return Results.Ok(Map(platformUser));
    }

    private static async Task<IResult> UpdatePhotoAsync(IFormFile file,
                                                        AppDbContext db,
                                                        IObjectStorageService storage,
                                                        ClaimsPrincipal user)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("Profile photo file is required.");

        var platformUser = await ResolvePlatformUserAsync(db, user);
        if (platformUser is null)
            return Results.Unauthorized();

        if (!string.IsNullOrWhiteSpace(platformUser.ProfilePhotoStorageKey))
            await storage.DeleteAsync(platformUser.ProfilePhotoStorageKey);

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        var normalizedFileName = $"owner_{platformUser.Id}{extension.ToLowerInvariant()}";
        var path = $"platform/owners/{platformUser.Id}/profile";

        using var stream = file.OpenReadStream();
        var uploadResult = await storage.UploadAsync(stream, normalizedFileName, file.ContentType, path);

        platformUser.ProfilePhotoUrl = uploadResult.Url;
        platformUser.ProfilePhotoStorageKey = uploadResult.Key;

        await db.SaveChangesAsync();
        return Results.Ok(Map(platformUser));
    }

    private static async Task<IResult> DeletePhotoAsync(AppDbContext db,
                                                        IObjectStorageService storage,
                                                        ClaimsPrincipal user)
    {
        var platformUser = await ResolvePlatformUserAsync(db, user);
        if (platformUser is null)
            return Results.Unauthorized();

        if (!string.IsNullOrWhiteSpace(platformUser.ProfilePhotoStorageKey))
            await storage.DeleteAsync(platformUser.ProfilePhotoStorageKey);

        platformUser.ProfilePhotoUrl = null;
        platformUser.ProfilePhotoStorageKey = null;

        await db.SaveChangesAsync();
        return Results.Ok(Map(platformUser));
    }

    private static async Task<PlatformUser?> ResolvePlatformUserAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue("platform_user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(claim, out var platformUserId))
            return null;

        return await db.PlatformUsers.FirstOrDefaultAsync(x => x.Id == platformUserId && x.IsActive);
    }

    private static PlatformProfileResponse Map(PlatformUser user)
    {
        return new PlatformProfileResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.MustChangePassword,
            user.ProfilePhotoUrl);
    }
}
