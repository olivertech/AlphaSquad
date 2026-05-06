namespace AlphaSquad.Api.Features.Tenants;

using AlphaSquad.Infrastructure.Persistence;
using AlphaSquad.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class TenantEndpoints
{
    /// <summary>
    /// O TenantEndpoints define os endpoints relacionados aos tenants.
    /// Organiza as rotas sob o prefixo (/api/tenants) e aplica tags para documentação.
    /// </summary>
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants");

        // Endpoint público para obter informações de um tenant por slug.
        group.MapGet("/by-slug/{slug}", GetBySlugAsync)
            .AllowAnonymous()
            .WithName("GetTenantBySlug")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Endpoint para atualizar as informações gerais do tenant.
        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization()
            .WithName("UpdateTenant")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Endpoint para obter as informações do Tenant do usuário autenticado.
        group.MapGet("/current", GetCurrentAsync)
            .RequireAuthorization()
            .WithName("GetCurrentTenant")
            .Produces<TenantCurrentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // Endpoint para atualizar a logo do Tenant atual.
        group.MapPut("/current/logo", UpdateLogoAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UpdateTenantLogo")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<TenantCurrentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        // Endpoint para obter as features habilitadas para o Tenant atual.
        group.MapGet("/current/features", GetCurrentFeaturesAsync)
            .RequireAuthorization()
            .WithName("GetCurrentTenantFeatures")
            .Produces<TenantFeaturesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        
        return app;
    }

    /// <summary>
    /// Retorna os dados do Tenant associados ao usuário autenticado.
    /// </summary>
    private static async Task<IResult> GetCurrentAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId);

        if (tenant is null)
            return Results.NotFound();

        return Results.Ok(new TenantCurrentResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            LogoUrl = tenant.LogoUrl,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            IsActive = tenant.IsActive
        });
    }

    /// <summary>
    /// Atualiza a logo do Tenant atual, removendo a anterior do storage e salvando a nova.
    /// </summary>
    private static async Task<IResult> UpdateLogoAsync(IFormFile file, AppDbContext db, IObjectStorageService storage, HttpContext context)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("Logo file is required.");

        var tenantId = context.GetTenantId();
        var tenantSlug = context.GetTenantSlug();

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId);
        if (tenant is null)
            return Results.NotFound();

        // 1. Remover a logo antiga do Storage se ela existir
        if (tenant.LogoMediaId.HasValue)
        {
            var oldLogo = await db.TenantMedias.FirstOrDefaultAsync(x => x.Id == tenant.LogoMediaId);
            if (oldLogo != null)
            {
                await storage.DeleteAsync(oldLogo.StorageKey);
                // Removemos o registro da mídia antiga para evitar lixo no banco
                db.TenantMedias.Remove(oldLogo);
            }
        }

        var path = $"tenants/{tenantSlug}/logos";
        using var stream = file.OpenReadStream();

        // 2. Fazer upload da nova logo
        var uploadResult = await storage.UploadAsync(
            stream,
            $"logo_{tenant.Id}.{Path.GetExtension(file.FileName)}",
            file.ContentType,
            path
        );

        // 3. Criar novo registro de mídia para a logo
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
        
        // 4. Atualizar a referência no Tenant
        tenant.LogoUrl = uploadResult.Url;
        tenant.LogoMediaId = logoMedia.Id;

        await db.SaveChangesAsync();

        return Results.Ok(new TenantCurrentResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            LogoUrl = tenant.LogoUrl,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            IsActive = tenant.IsActive
        });
    }

    /// <summary>
    /// Retorna a lista de features habilitadas para o Tenant do usuário autenticado.
    /// </summary>
    private static async Task<IResult> GetCurrentFeaturesAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var features = await db.TenantFeatures
            .Where(x => x.TenantId == tenantId)
            .Include(x => x.Feature)
            .Select(x => new TenantFeaturesResponse.FeatureItem
            {
                Name = x.Feature.Name,
                Description = x.Feature.Description
            })
            .ToListAsync();

        return Results.Ok(new TenantFeaturesResponse { Features = features });
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

        var response = new TenantConfigResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            LogoUrl = tenant.LogoUrl,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            IsActive = tenant.IsActive
        };

        return Results.Ok(response);
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
                                                                                        
        var tenant = await db.Tenants
            .AsNoTracking()
            .Where(x => x.Slug == normalizedSlug && x.IsActive)
            .Select(x => new TenantConfigResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                LogoUrl = x.LogoUrl,
                PrimaryColor = x.PrimaryColor,
                SecondaryColor = x.SecondaryColor,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        if (tenant is null)
            return Results.NotFound();

        await cache.SetAsync(
            cacheKey,
            tenant,
            TimeSpan.FromMinutes(30));

        return Results.Ok(tenant);
    }
}
