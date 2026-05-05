namespace AlphaSquad.Api.Features.Tenants;

/// <summary>
/// O TenantEndpoints define os endpoints relacionados aos tenants, como obter informações de um tenant por slug. 
/// Ele é usado para organizar e agrupar as rotas de tenants sob um prefixo 
/// comum (/api/tenants) e aplicar tags para documentação.
/// Com essa classe e método, o código de configuração dos endpoints de tenants 
/// fica centralizado e fácil de manter, além de melhorar a clareza e a organização do código da API.
/// No Program.cs, o método MapTenantEndpoints é chamado para registrar esses endpoints na aplicação, 
/// ao invés de chamar MapControllers, garantindo que as rotas de tenants estejam disponíveis para os clientes da API.
/// </summary>
/// <param name="app"></param>
/// <returns></returns>
public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants");

        // Endpoint para obter as informações de um tenant com base no slug fornecido. Ele é público (AllowAnonymous)
        // para permitir que clientes obtenham as informações do tenant sem necessidade de autenticação.
        group.MapGet("/by-slug/{slug}", GetBySlugAsync)
            .AllowAnonymous()
            .WithName("GetTenantBySlug")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Endpoint para atualizar as informações de um tenant existente. Ele recebe o ID do tenant a ser atualizado,
        // juntamente com os novos dados fornecidos no request. Ele requer autenticação (RequireAuthorization) para garantir
        // que apenas usuários autorizados possam atualizar as informações do tenant.
        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization()
            .WithName("UpdateTenant")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Endpoint para atualizar as informações de um tenant existente. Ele recebe o ID do tenant a ser atualizado,
    /// juntamente com os novos dados fornecidos no request.
    /// </summary>
    /// <param name="id">O ID do tenant a ser atualizado.</param>
    /// <param name="request">Os novos dados do tenant.</param>
    /// <param name="db">O contexto do banco de dados.</param>
    /// <param name="cache">O serviço de cache.</param>
    /// <returns>O resultado da operação de atualização.</returns>
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

        // Após atualizar o tenant, é importante remover a configuração antiga do cache para
        // garantir que as próximas requisições obtenham os dados atualizados do banco de dados.
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

    /// <summary>
    /// Endpoint para obter as informações de um tenant com base no slug fornecido.
    /// </summary>
    /// <param name="slug">O slug do tenant.</param>
    /// <param name="db">O contexto do banco de dados.</param>
    /// <param name="cache">O serviço de cache.</param>
    /// <returns>As informações do tenant ou um status de erro.</returns>
    private static async Task<IResult> GetBySlugAsync(string slug, AppDbContext db, IRedisCacheService cache)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return Results.BadRequest("Tenant slug is required.");

        var normalizedSlug = slug.Trim().ToLower();

        var cacheKey = CacheKeys.TenantConfig(normalizedSlug);

        var cachedTenant = await cache.GetAsync<TenantConfigResponse>(cacheKey);

        // Se a configuração do tenant estiver presente no cache, retorna-a imediatamente para melhorar a performance
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

        // Guarda a configuração do tenant no cache por 30 minutos para melhorar a performance em futuras requisições
        await cache.SetAsync(
            cacheKey,
            tenant,
            TimeSpan.FromMinutes(30));

        return Results.Ok(tenant);
    }
}