namespace AlphaSquad.Api.Features.Tenants;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants");

        group.MapGet("/by-slug/{slug}", GetBySlugAsync)
            .AllowAnonymous()
            .WithName("GetTenantBySlug")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization()
            .WithName("UpdateTenant")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/current", GetCurrentAsync)
            .RequireAuthorization()
            .WithName("GetCurrentTenant")
            .Produces<TenantCurrentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/current/logo", UpdateLogoAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UpdateTenantLogo")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<TenantCurrentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/current/features", GetCurrentFeaturesAsync)
            .RequireAuthorization()
            .WithName("GetCurrentTenantFeatures")
            .Produces<TenantFeaturesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        
        return app;
    }

    private static async Task<IResult> GetCurrentAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT id, 
                                    name, 
                                    slug, 
                                    logo_url AS LogoUrl, 
                                    primary_color AS PrimaryColor, 
                                    secondary_color AS SecondaryColor, 
                                    is_active AS IsActive 
                             FROM tenants 
                             WHERE id = @Id";

        var tenant = await connection.QueryFirstOrDefaultAsync<TenantCurrentResponse>(sql, new { Id = tenantId });

        if (tenant is null)
            return Results.NotFound();

        return Results.Ok(tenant);
    }

    private static async Task<IResult> UpdateLogoAsync(IFormFile file, AppDbContext db, IObjectStorageService storage, IRedisCacheService cache, HttpContext context)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("Logo file is required.");

        var tenantId = context.GetTenantId();
        var tenantSlug = context.GetTenantSlug();

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId);
        if (tenant is null)
            return Results.NotFound();

        if (tenant.LogoMediaId.HasValue)
        {
            var oldLogo = await db.TenantMedias.FirstOrDefaultAsync(x => x.Id == tenant.LogoMediaId);
            if (oldLogo != null)
            {
                await storage.DeleteAsync(oldLogo.StorageKey);
                db.TenantMedias.Remove(oldLogo);
            }
        }

        var path = $"tenants/{tenantSlug}/logos";
        using var stream = file.OpenReadStream();

        var uploadResult = await storage.UploadAsync(
            stream,
            $"logo_{tenant.Id}.{Path.GetExtension(file.FileName)}",
            file.ContentType,
            path
        );

        var logoMedia = new TenantMedia
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FileName = $"logo_{tenant.Id}.{Path.GetExtension(file.FileName)}",
            ContentType = file.ContentType,
            StorageKey = uploadResult.Key,
            Url = uploadResult.Url,
            CreatedAt = DateTime.UtcNow
        };

        db.TenantMedias.Add(logoMedia);
        
        tenant.LogoUrl = uploadResult.Url;
        tenant.LogoMediaId = logoMedia.Id;

        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.TenantConfig(tenantSlug));

        return Results.Ok(new TenantCurrentResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.LogoUrl,
            tenant.PrimaryColor,
            tenant.SecondaryColor,
            tenant.IsActive
        ));
    }

    private static async Task<IResult> GetCurrentFeaturesAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT f.name, 
                                    f.description 
                             FROM tenant_features tf
                             JOIN features f ON tf.feature_id = f.id
                             WHERE tf.tenant_id = @TenantId";

        var features = await connection.QueryAsync<TenantFeaturesResponse.FeatureItem>(sql, new { TenantId = tenantId });

        return Results.Ok(new TenantFeaturesResponse(features.ToList()));
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdateTenantRequest request, AppDbContext db, IRedisCacheService cache)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id);

        if (tenant is null)
            return Results.NotFound();

        var oldSlug = tenant.Slug;

        tenant.Name = request.Name.Trim();
        tenant.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl)
            ? null
            : request.LogoUrl.Trim();

        tenant.PrimaryColor = request.PrimaryColor.Trim();
        tenant.SecondaryColor = request.SecondaryColor.Trim();

        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.TenantConfig(oldSlug));

        return Results.Ok(new TenantConfigResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.LogoUrl,
            tenant.PrimaryColor,
            tenant.SecondaryColor,
            tenant.IsActive
        ));
    }

    private static async Task<IResult> GetBySlugAsync(string slug, AppDbContext db, IRedisCacheService cache)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return Results.BadRequest("Tenant slug is required.");

        var normalizedSlug = slug.Trim().ToLower();
        var cacheKey = CacheKeys.TenantConfig(normalizedSlug);
        var cachedTenant = await cache.GetAsync<TenantConfigResponse>(cacheKey);

        if (cachedTenant is not null)
            return Results.Ok(cachedTenant);
                                                                                        
        var connection = db.Database.GetDbConnection();
        const string sql = @"SELECT id, 
                                    name, 
                                    slug, 
                                    logo_url AS LogoUrl, 
                                    primary_color AS PrimaryColor, 
                                    secondary_color AS SecondaryColor, 
                                    is_active AS IsActive 
                             FROM tenants 
                             WHERE slug = @Slug AND is_active = true";

        var tenant = await connection.QueryFirstOrDefaultAsync<TenantConfigResponse>(sql, new { Slug = normalizedSlug });

        if (tenant is null)
            return Results.NotFound();

        await cache.SetAsync(
            cacheKey,
            tenant,
            TimeSpan.FromMinutes(30));

        return Results.Ok(tenant);
    }
}
