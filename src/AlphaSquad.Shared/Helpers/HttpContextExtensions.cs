namespace AlphaSquad.Shared.Helpers;

/// <summary>
/// Classe de extensão para HttpContext, que fornece métodos para 
/// extrair informações do contexto HTTP, como o TenantId e TenantSlug do token JWT.
/// </summary>
public static class HttpContextExtensions
{
    public static Guid GetTenantId(this HttpContext context)
    {
        var tenantId = context.User.FindFirst("tenant_id")?.Value;

        if (string.IsNullOrEmpty(tenantId))
            throw new Exception("Tenant not found in token");

        return Guid.Parse(tenantId);
    }

    public static string GetTenantSlug(this HttpContext context)
    {
        return context.User.FindFirst("tenant_slug")?.Value ?? throw new Exception("TenantSlug not found");
    }
}
