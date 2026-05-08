using AlphaSquad.Infrastructure.Auth;
using AlphaSquad.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaSquad.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly IBCryptPasswordHasher _passwordHasher;

    public DatabaseSeeder(AppDbContext db, IBCryptPasswordHasher passwordHasher)
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

        // Seed de Features
        await SeedFeaturesAsync(tenant);

        // Seed de planos base para o tenant demo
        await SeedMembershipPlansAsync(tenant);
    }

    private async Task SeedFeaturesAsync(Tenant tenant)
    {
        var featuresToSeed = new List<(string Name, string Description)>
        {
            ("WORKOUTS", "Gestão de treinos e exercícios."),
            ("CHECKIN", "Controle de entrada e frequência de alunos."),
            ("SCHEDULE", "Agendamento de aulas e horários."),
            ("MEDIA", "Gestão de mídias e arquivos do tenant."),
            ("USER_MGMT", "Gestão avançada de usuários e permissões.")
        };

        foreach (var featureData in featuresToSeed)
        {
            var feature = await _db.Features.FirstOrDefaultAsync(x => x.Name == featureData.Name);
            
            if (feature is null)
            {
                feature = new Feature
                {
                    Id = Guid.NewGuid(),
                    Name = featureData.Name,
                    Description = featureData.Description
                };
                _db.Features.Add(feature);
                await _db.SaveChangesAsync();
            }

            // Vincula a feature ao tenant demo se ainda não estiver vinculada
            var exists = await _db.TenantFeatures.AnyAsync(x => x.TenantId == tenant.Id && x.FeatureId == feature.Id);
            if (!exists)
            {
                _db.TenantFeatures.Add(new TenantFeature
                {
                    TenantId = tenant.Id,
                    FeatureId = feature.Id
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task SeedMembershipPlansAsync(Tenant tenant)
    {
        var plansToSeed = new List<(string Name, string Description, decimal Price, int DurationDays)>
        {
            ("Basic", "Basic plan", 120.00m, 365),
            ("Advanced", "Basic advanced", 145.00m, 365),
            ("Premium", "Basic premium", 220.00m, 365)
        };

        foreach (var planData in plansToSeed)
        {
            var existingPlan = await _db.MembershipPlans
                .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Name == planData.Name);

            if (existingPlan is not null)
            {
                continue;
            }

            _db.MembershipPlans.Add(new MembershipPlan
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = planData.Name,
                Description = planData.Description,
                Price = planData.Price,
                DurationDays = planData.DurationDays,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }
}

