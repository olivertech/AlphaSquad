namespace AlphaSquad.Api.Features.PlatformTenants;

/// <summary>
/// Registra os endpoints globais de gestão de academias no backoffice master da AlphaSquad.
/// Esse módulo permite criar novas academias, definir módulos e provisionar o admin inicial.
/// </summary>
public static class PlatformTenantEndpoints
{
    public static IEndpointRouteBuilder MapPlatformTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/platform-tenants")
            .WithTags("Platform Tenants")
            .RequireAuthorization(AuthorizationPolicies.PlatformOwnerOnly);

        group.MapGet("/features/catalog", GetFeatureCatalogAsync)
            .WithName("GetPlatformFeatureCatalog")
            .WithSummary("Lista o catálogo de módulos disponíveis para novas academias.")
            .Produces<List<PlatformFeatureCatalogItemResponse>>(StatusCodes.Status200OK);

        group.MapGet("/", GetAllAsync)
            .WithName("GetPlatformTenants")
            .WithSummary("Lista as academias cadastradas na AlphaSquad.")
            .Produces<List<PlatformTenantListItemResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetPlatformTenantById")
            .WithSummary("Retorna os detalhes administrativos de uma academia.")
            .Produces<PlatformTenantDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreatePlatformTenant")
            .WithSummary("Cria uma nova academia com branding, módulos e admin inicial.")
            .Produces<PlatformTenantProvisioningResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdatePlatformTenant")
            .WithSummary("Atualiza branding, status, módulos e admin principal da academia.")
            .Produces<PlatformTenantDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}/logo", UpdateLogoAsync)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("UpdatePlatformTenantLogo")
            .WithSummary("Atualiza a logo da academia pelo contexto master.")
            .Produces<PlatformTenantDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/reset-admin-password", ResetPrimaryAdminPasswordAsync)
            .WithName("ResetPlatformTenantPrimaryAdminPassword")
            .WithSummary("Gera uma nova senha provisória para o admin principal da academia.")
            .Produces<PlatformTenantAdminPasswordResetResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Expõe o catálogo oficial de features que o sponsor pode habilitar por academia.
    /// </summary>
    private static async Task<IResult> GetFeatureCatalogAsync(AppDbContext db)
    {
        var features = await db.Features
            .OrderBy(x => x.Name)
            .Select(x => new PlatformFeatureCatalogItemResponse(x.Id, x.Name, x.Description))
            .ToListAsync();

        return Results.Ok(features);
    }

    /// <summary>
    /// Lista todas as academias da plataforma com um resumo do admin principal e da quantidade de módulos.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db)
    {
        var tenants = await db.Tenants
            .OrderByDescending(x => x.CreatedAt)
            .Select(tenant => new PlatformTenantListItemResponse(
                tenant.Id,
                tenant.Name,
                tenant.Slug,
                tenant.LogoUrl,
                tenant.PrimaryColor,
                tenant.SecondaryColor,
                tenant.IsActive,
                tenant.CreatedAt,
                db.Users
                    .Where(user => user.TenantId == tenant.Id && user.Role == UserRole.Admin)
                    .OrderBy(user => user.CreatedAt)
                    .Select(user => user.Name)
                    .FirstOrDefault() ?? string.Empty,
                db.Users
                    .Where(user => user.TenantId == tenant.Id && user.Role == UserRole.Admin)
                    .OrderBy(user => user.CreatedAt)
                    .Select(user => user.Email)
                    .FirstOrDefault() ?? string.Empty,
                db.Users
                    .Where(user => user.TenantId == tenant.Id && user.Role == UserRole.Admin)
                    .OrderBy(user => user.CreatedAt)
                    .Select(user => user.MustChangePassword)
                    .FirstOrDefault(),
                db.TenantFeatures.Count(link => link.TenantId == tenant.Id)))
            .ToListAsync();

        return Results.Ok(tenants);
    }

    /// <summary>
    /// Retorna os detalhes administrativos completos de uma academia.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db)
    {
        var response = await BuildTenantDetailsResponseAsync(id, db);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    /// <summary>
    /// Cria uma nova academia, vincula os módulos escolhidos e provisiona o admin inicial com senha provisória.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreatePlatformTenantRequest request,
                                                   AppDbContext db,
                                                   IBCryptPasswordHasher passwordHasher)
    {
        var validation = await ValidateTenantRequestAsync(
            request.Name,
            request.Slug,
            request.PrimaryColor,
            request.SecondaryColor,
            request.AdminName,
            request.AdminEmail,
            request.FeatureCodes,
            db);

        if (validation is not null)
            return validation;

        var normalizedSlug = NormalizeSlug(request.Slug);
        var normalizedAdminEmail = NormalizeEmail(request.AdminEmail);
        var normalizedFeatureCodes = NormalizeFeatureCodes(request.FeatureCodes);

        var slugExists = await db.Tenants.AnyAsync(x => x.Slug == normalizedSlug);
        if (slugExists)
            return Results.Conflict("A tenant with this slug already exists.");

        var emailExists = await db.Users.AnyAsync(x => x.Email == normalizedAdminEmail);
        if (emailExists)
            return Results.Conflict("This administrator e-mail is already associated with another academy.");

        var features = await db.Features
            .Where(x => normalizedFeatureCodes.Contains(x.Name))
            .ToListAsync();

        if (features.Count != normalizedFeatureCodes.Count)
            return Results.BadRequest("One or more selected features are invalid.");

        var temporaryPassword = GenerateTemporaryPassword();

        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = normalizedSlug,
            LogoUrl = NormalizeOptionalText(request.LogoUrl),
            PrimaryColor = NormalizeColor(request.PrimaryColor),
            SecondaryColor = NormalizeColor(request.SecondaryColor),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Name = request.AdminName.Trim(),
            Email = normalizedAdminEmail,
            PasswordHash = passwordHasher.Hash(temporaryPassword),
            Role = UserRole.Admin,
            IsActive = true,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Tenants.Add(tenant);
        db.Users.Add(admin);

        foreach (var feature in features)
        {
            db.TenantFeatures.Add(new TenantFeature
            {
                TenantId = tenant.Id,
                FeatureId = feature.Id
            });
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        var response = await BuildTenantDetailsResponseAsync(tenant.Id, db);
        return Results.Created($"/api/platform-tenants/{tenant.Id}", new PlatformTenantProvisioningResponse(
            response!,
            temporaryPassword,
            true));
    }

    /// <summary>
    /// Atualiza branding, status, módulos e dados do admin principal da academia.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id,
                                                   UpdatePlatformTenantRequest request,
                                                   AppDbContext db)
    {
        var validation = await ValidateTenantRequestAsync(
            request.Name,
            request.Slug,
            request.PrimaryColor,
            request.SecondaryColor,
            request.AdminName,
            request.AdminEmail,
            request.FeatureCodes,
            db);

        if (validation is not null)
            return validation;

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id);
        if (tenant is null)
            return Results.NotFound();

        var normalizedSlug = NormalizeSlug(request.Slug);
        var normalizedAdminEmail = NormalizeEmail(request.AdminEmail);
        var normalizedFeatureCodes = NormalizeFeatureCodes(request.FeatureCodes);

        var slugExists = await db.Tenants.AnyAsync(x => x.Id != id && x.Slug == normalizedSlug);
        if (slugExists)
            return Results.Conflict("A tenant with this slug already exists.");

        var primaryAdmin = await db.Users
            .Where(x => x.TenantId == id && x.Role == UserRole.Admin)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (primaryAdmin is null)
            return Results.BadRequest("This academy does not have a primary administrator to update.");

        var adminEmailExists = await db.Users.AnyAsync(x =>
            x.Id != primaryAdmin.Id &&
            x.Email == normalizedAdminEmail);

        if (adminEmailExists)
            return Results.Conflict("This administrator e-mail is already associated with another academy.");

        var features = await db.Features
            .Where(x => normalizedFeatureCodes.Contains(x.Name))
            .ToListAsync();

        if (features.Count != normalizedFeatureCodes.Count)
            return Results.BadRequest("One or more selected features are invalid.");

        tenant.Name = request.Name.Trim();
        tenant.Slug = normalizedSlug;
        if (request.LogoUrl is not null)
            tenant.LogoUrl = NormalizeOptionalText(request.LogoUrl);
        tenant.PrimaryColor = NormalizeColor(request.PrimaryColor);
        tenant.SecondaryColor = NormalizeColor(request.SecondaryColor);
        tenant.IsActive = request.IsActive;

        primaryAdmin.Name = request.AdminName.Trim();
        primaryAdmin.Email = normalizedAdminEmail;

        var currentLinks = await db.TenantFeatures.Where(x => x.TenantId == id).ToListAsync();
        db.TenantFeatures.RemoveRange(currentLinks);

        foreach (var feature in features)
        {
            db.TenantFeatures.Add(new TenantFeature
            {
                TenantId = tenant.Id,
                FeatureId = feature.Id
            });
        }

        await db.SaveChangesAsync();

        var response = await BuildTenantDetailsResponseAsync(id, db);
        return Results.Ok(response!);
    }

    /// <summary>
    /// Atualiza a logo da academia usando o mesmo storage do tenant, mas sob controle do owner.
    /// A operacao preserva o vinculo com TenantMedia para manter a limpeza correta do arquivo anterior.
    /// </summary>
    private static async Task<IResult> UpdateLogoAsync(Guid id,
                                                       IFormFile file,
                                                       AppDbContext db,
                                                       IObjectStorageService storage)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("Logo file is required.");

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id);
        if (tenant is null)
            return Results.NotFound();

        if (tenant.LogoMediaId.HasValue)
        {
            var oldLogo = await db.TenantMedias.FirstOrDefaultAsync(x => x.Id == tenant.LogoMediaId && x.TenantId == tenant.Id);
            if (oldLogo != null)
            {
                await storage.DeleteAsync(oldLogo.StorageKey);
                db.TenantMedias.Remove(oldLogo);
            }
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".png";

        var normalizedFileName = $"logo_{tenant.Id}{extension.ToLowerInvariant()}";
        var path = $"tenants/{tenant.Slug}/logos";

        using var stream = file.OpenReadStream();
        var uploadResult = await storage.UploadAsync(stream, normalizedFileName, file.ContentType, path);

        var logoMedia = new TenantMedia
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            FileName = normalizedFileName,
            ContentType = file.ContentType,
            StorageKey = uploadResult.Key,
            Url = uploadResult.Url,
            Size = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        db.TenantMedias.Add(logoMedia);
        tenant.LogoUrl = uploadResult.Url;
        tenant.LogoMediaId = logoMedia.Id;

        await db.SaveChangesAsync();

        var response = await BuildTenantDetailsResponseAsync(tenant.Id, db);
        return Results.Ok(response!);
    }

    /// <summary>
    /// Gera uma nova senha provisória para o admin principal da academia.
    /// O usuário passa a ser obrigado a trocar a senha na próxima entrada.
    /// </summary>
    private static async Task<IResult> ResetPrimaryAdminPasswordAsync(Guid id,
                                                                      AppDbContext db,
                                                                      IBCryptPasswordHasher passwordHasher)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == id);
        if (tenant is null)
            return Results.NotFound();

        var primaryAdmin = await db.Users
            .Where(x => x.TenantId == id && x.Role == UserRole.Admin)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (primaryAdmin is null)
            return Results.NotFound();

        var temporaryPassword = GenerateTemporaryPassword();
        primaryAdmin.PasswordHash = passwordHasher.Hash(temporaryPassword);
        primaryAdmin.MustChangePassword = true;

        var tokens = await db.RefreshTokens.Where(x => x.UserId == primaryAdmin.Id && !x.IsRevoked).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await db.SaveChangesAsync();

        return Results.Ok(new PlatformTenantAdminPasswordResetResponse(
            tenant.Id,
            primaryAdmin.Id,
            primaryAdmin.Email,
            temporaryPassword,
            true));
    }

    private static async Task<PlatformTenantDetailsResponse?> BuildTenantDetailsResponseAsync(Guid tenantId, AppDbContext db)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId);
        if (tenant is null)
            return null;

        var primaryAdmin = await db.Users
            .Where(x => x.TenantId == tenantId && x.Role == UserRole.Admin)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PlatformTenantAdminResponse(
                x.Id,
                x.Name,
                x.Email,
                x.IsActive,
                x.MustChangePassword,
                x.CreatedAt))
            .FirstOrDefaultAsync();

        var features = await db.TenantFeatures
            .Where(x => x.TenantId == tenantId)
            .Join(db.Features,
                link => link.FeatureId,
                feature => feature.Id,
                (link, feature) => new PlatformFeatureCatalogItemResponse(feature.Id, feature.Name, feature.Description))
            .OrderBy(x => x.Code)
            .ToListAsync();

        return new PlatformTenantDetailsResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.LogoUrl,
            tenant.PrimaryColor,
            tenant.SecondaryColor,
            tenant.IsActive,
            tenant.CreatedAt,
            primaryAdmin,
            features);
    }

    private static async Task<IResult?> ValidateTenantRequestAsync(string name,
                                                                   string slug,
                                                                   string primaryColor,
                                                                   string secondaryColor,
                                                                   string adminName,
                                                                   string adminEmail,
                                                                   IReadOnlyCollection<string>? featureCodes,
                                                                   AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Tenant name is required.");

        if (string.IsNullOrWhiteSpace(slug))
            return Results.BadRequest("Tenant slug is required.");

        if (NormalizeSlug(slug).Length < 3)
            return Results.BadRequest("Tenant slug must have at least 3 characters.");

        if (string.IsNullOrWhiteSpace(primaryColor) || string.IsNullOrWhiteSpace(secondaryColor))
            return Results.BadRequest("Primary and secondary colors are required.");

        if (string.IsNullOrWhiteSpace(adminName))
            return Results.BadRequest("Primary administrator name is required.");

        if (string.IsNullOrWhiteSpace(adminEmail))
            return Results.BadRequest("Primary administrator e-mail is required.");

        try
        {
            _ = new System.Net.Mail.MailAddress(adminEmail.Trim());
        }
        catch
        {
            return Results.BadRequest("Primary administrator e-mail is invalid.");
        }

        if (featureCodes is null || featureCodes.Count == 0)
            return Results.BadRequest("Select at least one feature for the academy.");

        var normalizedFeatureCodes = NormalizeFeatureCodes(featureCodes);
        var existingFeatureCount = await db.Features.CountAsync(x => normalizedFeatureCodes.Contains(x.Name));
        if (existingFeatureCount != normalizedFeatureCodes.Count)
            return Results.BadRequest("One or more selected features are invalid.");

        return null;
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeColor(string color)
    {
        var normalized = color.Trim();
        return normalized.StartsWith('#') ? normalized : $"#{normalized}";
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<string> NormalizeFeatureCodes(IEnumerable<string> featureCodes)
    {
        return featureCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
