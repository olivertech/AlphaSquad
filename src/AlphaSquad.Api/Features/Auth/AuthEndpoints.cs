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
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", MeAsync)
            .RequireAuthorization()
            .WithName("Me")
            .Produces<AuthenticatedUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("Refresh")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized);

        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);
        
        return app;
    }

    private static IResult MeAsync(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = user.FindFirstValue(ClaimTypes.Name);
        var email = user.FindFirstValue(ClaimTypes.Email);
        var role = user.FindFirstValue(ClaimTypes.Role);
        var tenantId = user.FindFirstValue("tenant_id");
        var tenantSlug = user.FindFirstValue("tenant_slug");

        return Results.Ok(new
        {
            UserId = userId,
            Name = name,
            Email = email,
            Role = role,
            TenantId = tenantId,
            TenantSlug = tenantSlug
        });
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
            new AuthenticatedUserResponse(user.Id, user.Name, user.Email, user.Role),
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

        var refreshToken = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken && !x.IsUsed && !x.IsRevoked);
        if (refreshToken is null)
            return Results.Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == refreshToken.UserId);
        if (user is null)
            return Results.Unauthorized();

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == user.TenantId);
        if (tenant is null)
            return Results.Unauthorized();

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
            new AuthenticatedUserResponse(user.Id, user.Name, user.Email, user.Role),
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
}
