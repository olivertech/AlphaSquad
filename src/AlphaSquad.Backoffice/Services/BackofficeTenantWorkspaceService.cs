using System.Collections.Concurrent;

namespace AlphaSquad.Backoffice.Services;

/// <summary>
/// Implementacao provisoria em memoria do modulo de academias do backoffice.
/// Ela deixa a experiencia navegavel antes da API master existir.
/// </summary>
public sealed class BackofficeTenantWorkspaceService : IBackofficeTenantWorkspaceService
{
    private readonly ConcurrentDictionary<Guid, BackofficeTenantWorkspaceItem> _tenants = new();

    public BackofficeTenantWorkspaceService()
    {
        var seed = new BackofficeTenantWorkspaceItem
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Academia Strong Fit",
            Slug = "strong-fit",
            PrimaryColor = "#2563eb",
            SecondaryColor = "#14b8a6",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-12),
            FeatureCodes =
            [
                "USER_MANAGEMENT",
                "CHECKIN",
                "SCHEDULE",
                "WORKOUTS",
                "EVENTS",
                "SOCIAL",
                "GAMIFICATION",
                "STORE",
                "MEDIA"
            ],
            PrimaryAdminName = "Marcela Lima",
            PrimaryAdminEmail = "admin@strongfit.app",
            TemporaryPassword = GenerateTemporaryPassword(),
            MustChangePassword = true
        };

        _tenants[seed.Id] = seed;
    }

    public Task<IReadOnlyList<BackofficeTenantWorkspaceItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenants = _tenants.Values
            .OrderByDescending(item => item.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<BackofficeTenantWorkspaceItem>>(tenants);
    }

    public Task<BackofficeTenantWorkspaceItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _tenants.TryGetValue(id, out var tenant);
        return Task.FromResult(tenant);
    }

    public Task<BackofficeTenantWorkspaceItem> CreateAsync(BackofficeTenantCreateCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = NormalizeSlug(command.Slug);

        if (_tenants.Values.Any(item => string.Equals(item.Slug, normalizedSlug, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ja existe uma academia cadastrada com esse slug.");

        if (_tenants.Values.Any(item => string.Equals(item.PrimaryAdminEmail, command.PrimaryAdminEmail.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ja existe uma academia vinculada a esse e-mail principal.");

        var tenant = new BackofficeTenantWorkspaceItem
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Slug = normalizedSlug,
            LogoUrl = command.LogoUrl,
            PrimaryColor = command.PrimaryColor.Trim(),
            SecondaryColor = command.SecondaryColor.Trim(),
            IsActive = command.IsActive,
            CreatedAt = DateTime.UtcNow,
            FeatureCodes = command.FeatureCodes.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            PrimaryAdminName = command.PrimaryAdminName.Trim(),
            PrimaryAdminEmail = command.PrimaryAdminEmail.Trim().ToLowerInvariant(),
            TemporaryPassword = GenerateTemporaryPassword(),
            MustChangePassword = true
        };

        _tenants[tenant.Id] = tenant;
        return Task.FromResult(tenant);
    }

    public Task<BackofficeTenantWorkspaceItem?> UpdateAsync(Guid id, BackofficeTenantUpdateCommand command, CancellationToken cancellationToken = default)
    {
        if (!_tenants.TryGetValue(id, out var tenant))
            return Task.FromResult<BackofficeTenantWorkspaceItem?>(null);

        var normalizedSlug = NormalizeSlug(command.Slug);
        var slugConflict = _tenants.Values.Any(item =>
            item.Id != id &&
            string.Equals(item.Slug, normalizedSlug, StringComparison.OrdinalIgnoreCase));

        if (slugConflict)
            throw new InvalidOperationException("Ja existe outra academia cadastrada com esse slug.");

        tenant.Name = command.Name.Trim();
        tenant.Slug = normalizedSlug;
        tenant.PrimaryColor = command.PrimaryColor.Trim();
        tenant.SecondaryColor = command.SecondaryColor.Trim();
        tenant.IsActive = command.IsActive;
        tenant.FeatureCodes = command.FeatureCodes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        if (!string.IsNullOrWhiteSpace(command.LogoUrl))
            tenant.LogoUrl = command.LogoUrl;

        return Task.FromResult<BackofficeTenantWorkspaceItem?>(tenant);
    }

    private static string NormalizeSlug(string slug)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        normalized = normalized.Replace(" ", "-");
        return normalized;
    }

    private static string GenerateTemporaryPassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var random = new Random();

        return new string(Enumerable.Range(0, 10)
            .Select(_ => alphabet[random.Next(alphabet.Length)])
            .ToArray());
    }
}
