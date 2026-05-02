using AlphaSquad.Infrastructure.Auth;
using AlphaSquad.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaSquad.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(AppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        var tenantSlug = "alpha-demo";

        var tenant = await _db.Tenants.FirstOrDefaultAsync(x => x.Slug == tenantSlug);

        // Se o tenant não existir, cria um novo tenant e um usuário admin para ele
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "AlphaSquad Client Demo",
                Slug = tenantSlug,
                LogoUrl = null,
                PrimaryColor = "#111827",
                SecondaryColor = "#2563EB",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();
        }

        var adminEmail = "admin@alphasquad.app";

        var adminExists = await _db.Users.AnyAsync(x => x.TenantId == tenant.Id && x.Email == adminEmail);

        // Se o usuário admin não existir, cria um novo usuário admin para o tenant demo
        if (!adminExists)
        {
            var admin = new AppUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Admin AlphaSquad Client Demo",
                Email = adminEmail,
                PasswordHash = _passwordHasher.Hash("123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(admin);
            await _db.SaveChangesAsync();
        }
    }
}