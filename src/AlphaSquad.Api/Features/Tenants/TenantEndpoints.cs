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

        group.MapGet("/by-slug/{slug}", GetBySlugAsync)
            .AllowAnonymous()
            .WithName("GetTenantBySlug")
            .Produces<TenantConfigResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetBySlugAsync(string slug, AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return Results.BadRequest("Tenant slug is required.");

        var normalizedSlug = slug.Trim().ToLower();

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

        return Results.Ok(tenant);
    }
}