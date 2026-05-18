namespace AlphaSquad.Api.Middleware;

/// <summary>
/// Resolve e valida o contexto do tenant para requisições autenticadas do mundo das academias.
/// O middleware ignora endpoints anônimos, rotas do backoffice master e chamadas sem claim de tenant.
/// </summary>
public sealed class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        // O contexto master da plataforma nao participa do fluxo multi-tenant das academias.
        if (context.User.HasClaim("platform_scope", PlatformClaimValues.Owner))
        {
            await next(context);
            return;
        }

        var tenantIdRaw = context.User.FindFirstValue("tenant_id");
        if (string.IsNullOrWhiteSpace(tenantIdRaw))
        {
            await next(context);
            return;
        }

        if (!Guid.TryParse(tenantIdRaw, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Tenant claim is invalid.");
            return;
        }

        var userIdRaw = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("User claim is invalid.");
            return;
        }

        var resolved = await db.Users
            .Where(user => user.Id == userId && user.TenantId == tenantId)
            .Join(db.Tenants,
                user => user.TenantId,
                tenant => tenant.Id,
                (user, tenant) => new TenantRequestContext
                {
                    TenantId = tenant.Id,
                    TenantSlug = tenant.Slug,
                    TenantName = tenant.Name,
                    IsTenantActive = tenant.IsActive,
                    UserId = user.Id,
                    UserName = user.Name,
                    UserEmail = user.Email,
                    UserRole = user.Role,
                    IsUserActive = user.IsActive
                })
            .FirstOrDefaultAsync();

        if (resolved is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Tenant session is no longer valid.");
            return;
        }

        if (!resolved.IsTenantActive)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Tenant is inactive.");
            return;
        }

        if (!resolved.IsUserActive)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("User is inactive.");
            return;
        }

        context.SetTenantContext(resolved);
        await next(context);
    }
}
