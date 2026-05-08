namespace AlphaSquad.Api.Features.Auth;

public static class AuthEndpoints
{
    /// <summary>
    /// O MapAuthEndpoints define os endpoints relacionados à autenticação, como login. 
    /// Ele é usado para organizar e agrupar as rotas de autenticação sob um prefixo 
    /// comum (/api/auth) e aplicar tags para documentação.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Autentica um usuário no tenant informado.")
            .WithDescription("Valida tenant, e-mail e senha e retorna access token, refresh token e dados básicos do usuário autenticado.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", MeAsync)
            .RequireAuthorization()
            .WithName("Me")
            .WithSummary("Retorna os dados do usuário autenticado.")
            .WithDescription("Retorna os dados atuais do usuário autenticado, combinando claims de sessão com informações persistidas de profile.")
            .Produces<AuthenticatedSessionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("Refresh")
            .WithSummary("Renova a sessão a partir de um refresh token válido.")
            .WithDescription("Valida o refresh token, aplica rotação do token e devolve um novo par de tokens de autenticação.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized);

        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithSummary("Altera a senha do usuário autenticado.")
            .WithDescription("Exige a senha atual, grava a nova senha com hash e revoga as sessões ativas do usuário.")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Encerra a sessão atual do usuário.")
            .WithDescription("Revoga os refresh tokens ativos do usuário autenticado para impedir novas renovações de sessão.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);
        
        return app;
    }

    private static async Task<IResult> MeAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = user.FindFirstValue("tenant_id");
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tenantId))
            return Results.Unauthorized();

        var authenticatedUser = await BuildAuthenticatedUserResponseAsync(Guid.Parse(userId), Guid.Parse(tenantId), db);
        if (authenticatedUser is null)
            return Results.Unauthorized();

        return Results.Ok(new AuthenticatedSessionResponse(
            authenticatedUser.Id,
            authenticatedUser.Name,
            authenticatedUser.Email,
            authenticatedUser.Role,
            authenticatedUser.Username,
            authenticatedUser.ProfilePhotoUrl,
            authenticatedUser.ActivePlanId,
            authenticatedUser.ActivePlan,
            authenticatedUser.ActivePlanPrice,
            authenticatedUser.ActivePlanDurationDays,
            Guid.Parse(tenantId),
            user.FindFirstValue("tenant_slug") ?? string.Empty
        ));
    }

    private static async Task<IResult> LoginAsync([FromBody] LoginRequest request,
                                                  AppDbContext db,
                                                  IBCryptPasswordHasher passwordHasher,
                                                  IJwtService jwtService)
    {
        if (string.IsNullOrWhiteSpace(request.TenantSlug) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest("Tenant slug, email and password are required.");

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == request.TenantSlug.Trim().ToLower() && x.IsActive);
        if (tenant is null)
            return Results.NotFound("Tenant not found or inactive.");

        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLower() && x.TenantId == tenant.Id && x.IsActive);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Results.Unauthorized();

        var hasActiveMembership = await HasActiveMembershipAsync(user.Id, tenant.Id, db);
        if (!hasActiveMembership)
            return Results.Json(new { message = "Only users with an active membership plan can access the platform." }, statusCode: StatusCodes.Status401Unauthorized);

        var accessToken = jwtService.GenerateAccessToken(user, tenant, out var expiresAt);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        var refreshToken = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == newRefreshToken);
        if (refreshToken != null)
            return Results.BadRequest("Token generation error.");

        var nextRefreshCid = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        db.RefreshTokens.Add(nextRefreshCid);
        await db.SaveChangesAsync();

        var response = new LoginResponse(
            accessToken,
            expiresAt,
            newRefreshToken,
            (await BuildAuthenticatedUserResponseAsync(user.Id, tenant.Id, db))!,
            new AuthenticatedTenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.LogoUrl, tenant.PrimaryColor, tenant.SecondaryColor)
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshAsync([FromBody] RefreshRequest request,
                                                  AppDbContext db,
                                                  IJwtService jwtService)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Results.BadRequest("Refresh token is required.");

        var refreshToken = await db.RefreshTokens.FirstOrDefaultAsync(x =>
            x.Token == request.RefreshToken &&
            !x.IsUsed &&
            !x.IsRevoked &&
            x.ExpiryDate > DateTime.UtcNow);
        if (refreshToken is null)
            return Results.Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == refreshToken.UserId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == user.TenantId && x.IsActive);
        if (tenant is null)
            return Results.Unauthorized();

        var hasActiveMembership = await HasActiveMembershipAsync(user.Id, tenant.Id, db);
        if (!hasActiveMembership)
            return Results.Json(new { message = "Only users with an active membership plan can access the platform." }, statusCode: StatusCodes.Status401Unauthorized);

        var accessToken = jwtService.GenerateAccessToken(user, tenant, out var expiresAt);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        refreshToken.IsUsed = true;

        var nextRefreshCid = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        db.RefreshTokens.Add(nextRefreshCid);
        await db.SaveChangesAsync();

        var response = new LoginResponse(
            accessToken,
            expiresAt,
            newRefreshToken,
            (await BuildAuthenticatedUserResponseAsync(user.Id, tenant.Id, db))!,
            new AuthenticatedTenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.LogoUrl, tenant.PrimaryColor, tenant.SecondaryColor)
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request,
                                                         AppDbContext db,
                                                         IBCryptPasswordHasher passwordHasher,
                                                         ClaimsPrincipal user)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return Results.BadRequest("Current and new passwords are required.");

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Results.Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var appUser = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (appUser is null)
            return Results.NotFound("User not found.");

        if (!passwordHasher.Verify(request.CurrentPassword, appUser.PasswordHash))
            return Results.BadRequest("Current password is incorrect.");

        appUser.PasswordHash = passwordHasher.Hash(request.NewPassword);
        
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await db.SaveChangesAsync();

        return Results.Ok("Password changed successfully and all active sessions were terminated.");
    }

    private static async Task<IResult> LogoutAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
            return Results.Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync();
        
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Reconstroi a visao autenticada do usuario unindo dados centrais e dados de profile.
    /// </summary>
    private static async Task<AuthenticatedUserResponse?> BuildAuthenticatedUserResponseAsync(Guid userId, Guid tenantId, AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT u.id,
                                    u.name,
                                    u.email,
                                    u.role,
                                    up.username,
                                    up.profile_photo_url AS ProfilePhotoUrl,
                                    active_plan.membership_plan_id AS ActivePlanId,
                                    active_plan.name AS ActivePlan,
                                    active_plan.price AS ActivePlanPrice,
                                    active_plan.duration_days AS ActivePlanDurationDays
                             FROM users u
                             LEFT JOIN user_profiles up
                               ON up.user_id = u.id
                              AND up.tenant_id = u.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT um.membership_plan_id,
                                        mp.name,
                                        mp.price,
                                        mp.duration_days
                                 FROM user_memberships um
                                 JOIN membership_plans mp
                                   ON mp.id = um.membership_plan_id
                                  AND mp.tenant_id = um.tenant_id
                                 WHERE um.user_id = u.id
                                   AND um.tenant_id = u.tenant_id
                                   AND um.is_active = true
                                   AND mp.is_active = true
                                   AND um.starts_at <= @Now
                                   AND (um.ends_at IS NULL OR um.ends_at > @Now)
                                 ORDER BY um.starts_at DESC, um.created_at DESC
                                 LIMIT 1
                             ) active_plan ON true
                             WHERE u.id = @UserId
                               AND u.tenant_id = @TenantId
                               AND u.is_active = true";

        return await connection.QueryFirstOrDefaultAsync<AuthenticatedUserResponse>(sql, new
        {
            UserId = userId,
            TenantId = tenantId,
            Now = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Verifica se o usuario possui um unico vinculo de plano atualmente valido para acessar a plataforma.
    /// O plano precisa estar ativo, dentro da vigencia e no mesmo tenant do usuario.
    /// </summary>
    private static async Task<bool> HasActiveMembershipAsync(Guid userId, Guid tenantId, AppDbContext db)
    {
        var now = DateTime.UtcNow;

        return await db.UserMemberships
            .Join(db.MembershipPlans,
                membership => new { membership.MembershipPlanId, membership.TenantId },
                plan => new { MembershipPlanId = plan.Id, plan.TenantId },
                (membership, plan) => new { membership, plan })
            .AnyAsync(x =>
                x.membership.UserId == userId &&
                x.membership.TenantId == tenantId &&
                x.membership.IsActive &&
                x.plan.IsActive &&
                x.membership.StartsAt <= now &&
                (!x.membership.EndsAt.HasValue || x.membership.EndsAt > now));
    }
}
