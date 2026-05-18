namespace AlphaSquad.Shared.Helpers;

/// <summary>
/// Classe de extensão para HttpContext, que fornece métodos para 
/// extrair informações do contexto HTTP, como o TenantId e TenantSlug do token JWT.
/// </summary>
public static class HttpContextExtensions
{
    private const string TenantContextItemKey = "AlphaSquad.TenantContext";

    public static Guid GetTenantId(this HttpContext context)
    {
        var tenantContext = context.TryGetTenantContext();
        if (tenantContext is not null)
            return tenantContext.TenantId;

        var tenantId = context.User.FindFirst("tenant_id")?.Value;

        if (string.IsNullOrEmpty(tenantId))
            throw new Exception("Tenant not found in token");

        return Guid.Parse(tenantId);
    }

    public static string GetTenantSlug(this HttpContext context)
    {
        var tenantContext = context.TryGetTenantContext();
        if (tenantContext is not null)
            return tenantContext.TenantSlug;

        return context.User.FindFirst("tenant_slug")?.Value ?? throw new Exception("TenantSlug not found");
    }

    public static string GetTenantName(this HttpContext context)
    {
        return context.GetTenantContext().TenantName;
    }

    public static Guid GetTenantUserId(this HttpContext context)
    {
        return context.GetTenantContext().UserId;
    }

    public static TenantRequestContext GetTenantContext(this HttpContext context)
    {
        return context.TryGetTenantContext() ?? throw new Exception("Resolved tenant context not found.");
    }

    public static TenantRequestContext? TryGetTenantContext(this HttpContext context)
    {
        if (context.Items.TryGetValue(TenantContextItemKey, out var value) && value is TenantRequestContext tenantContext)
            return tenantContext;

        return null;
    }

    public static void SetTenantContext(this HttpContext context, TenantRequestContext tenantContext)
    {
        context.Items[TenantContextItemKey] = tenantContext;
    }
}
