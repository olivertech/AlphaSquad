namespace AlphaSquad.Infrastructure.Caching;

public static class CacheKeys
{
    public static string TenantConfig(string slug)
    {
        return $"tenant-config:{slug.Trim().ToLower()}";
    }
}
