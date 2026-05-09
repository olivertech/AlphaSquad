namespace AlphaSquad.Api.Features.Tenants;

public static class TenantEndpoints
{
    /// <summary>
    /// Registra os endpoints de tenant e branding da plataforma.
    /// O grupo mistura consultas publicas por slug com operacoes autenticadas do tenant atual.
    /// </summary>
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants");

        group.MapGet("/by-slug/{slug}", GetBySlugAsync)
            .AllowAnonymous()
            .WithName("GetTenantBySlug")
            .WithSummary("Busca a configuração pública de um tenant pelo slug.")
            .WithDescription("Retorna dados básicos de branding e ativação do tenant, com apoio de cache Redis.")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateTenant")
            .WithSummary("Atualiza os dados do tenant autenticado.")
            .WithDescription("Permite alterar nome, logo e cores do tenant atual, respeitando o isolamento multi-tenant.")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/current", GetCurrentAsync)
            .RequireAuthorization()
            .WithName("GetCurrentTenant")
            .WithSummary("Retorna o tenant da sessão atual.")
            .WithDescription("Consulta os dados completos do tenant associado ao token JWT enviado na requisição.")
            .Produces<TenantCurrentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/current/logo", UpdateLogoAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .DisableAntiforgery()
            .WithName("UpdateTenantLogo")
            .WithSummary("Atualiza a logo do tenant atual.")
            .WithDescription("Faz upload da nova logo no storage, remove o arquivo anterior quando existir e atualiza o vínculo do tenant.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<TenantCurrentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/current/features", GetCurrentFeaturesAsync)
            .RequireAuthorization()
            .WithName("GetCurrentTenantFeatures")
            .WithSummary("Lista as features habilitadas para o tenant atual.")
            .WithDescription("Retorna os módulos e capacidades liberados para o tenant associado à sessão atual.")
            .Produces<TenantFeaturesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        
        return app;
    }

    /// <summary>
    /// Retorna os dados completos do tenant da sessao atual.
    /// </summary>
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

    /// <summary>
    /// Atualiza a logo do tenant atual no storage e no banco.
    /// Quando ja existir uma logo anterior, o arquivo antigo e removido para evitar lixo de storage.
    /// </summary>
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
            var oldLogo = await db.TenantMedias.FirstOrDefaultAsync(x => x.Id == tenant.LogoMediaId && x.TenantId == tenantId);
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

    /// <summary>
    /// Lista as features habilitadas para o tenant atual.
    /// Essa consulta apoia tanto o app cliente quanto o controle de modulos opcionais.
    /// </summary>
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

    /// <summary>
    /// Atualiza os dados basicos do tenant autenticado.
    /// O filtro por `id` e `tenantId` impede que um administrador altere outro tenant por engano ou abuso.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id, UpdateTenantRequest request, AppDbContext db, IRedisCacheService cache, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id && x.Id == tenantId);

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

    /// <summary>
    /// Resolve a configuracao publica de um tenant pelo slug.
    /// Essa consulta usa cache Redis para reduzir leituras repetidas de branding no bootstrap do app cliente.
    /// </summary>
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
