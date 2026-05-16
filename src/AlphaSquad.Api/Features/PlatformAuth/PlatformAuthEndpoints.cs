namespace AlphaSquad.Api.Features.PlatformAuth;

/// <summary>
/// Registra os endpoints de autenticacao do backoffice master da AlphaSquad.
/// Esse fluxo fica separado do login das academias para preservar o isolamento multi-tenant da plataforma.
/// </summary>
public static class PlatformAuthEndpoints
{
    public static IEndpointRouteBuilder MapPlatformAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/platform-auth")
            .WithTags("Platform Auth");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("PlatformLogin")
            .WithSummary("Autentica o sponsor da AlphaSquad no backoffice master.")
            .Produces<PlatformLoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", MeAsync)
            .RequireAuthorization(AuthorizationPolicies.PlatformOwnerOnly)
            .WithName("PlatformMe")
            .WithSummary("Retorna os dados do usuário master autenticado.")
            .Produces<PlatformAuthenticatedSessionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("PlatformRefresh")
            .WithSummary("Renova a sessão do backoffice master a partir de um refresh token válido.")
            .Produces<PlatformLoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization(AuthorizationPolicies.PlatformOwnerOnly)
            .WithName("PlatformChangePassword")
            .WithSummary("Altera a senha do usuário master autenticado.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization(AuthorizationPolicies.PlatformOwnerOnly)
            .WithName("PlatformLogout")
            .WithSummary("Revoga os refresh tokens ativos do usuário master autenticado.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    /// <summary>
    /// Executa o login do sponsor usando uma conta global da AlphaSquad.
    /// O token resultante nao leva tenant_id, apenas claims do contexto master.
    /// </summary>
    private static async Task<IResult> LoginAsync([FromBody] PlatformLoginRequest request,
                                                  AppDbContext db,
                                                  IBCryptPasswordHasher passwordHasher,
                                                  IPlatformJwtService jwtService)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest("Email and password are required.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await db.PlatformUsers.FirstOrDefaultAsync(x => x.Email == normalizedEmail && x.IsActive);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Results.Unauthorized();

        var accessToken = jwtService.GenerateAccessToken(user, out var expiresAt);
        var refreshTokenValue = jwtService.GenerateRefreshToken();

        if (await db.PlatformRefreshTokens.AnyAsync(x => x.Token == refreshTokenValue))
            return Results.BadRequest("Token generation error.");

        db.PlatformRefreshTokens.Add(new PlatformRefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            PlatformUserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        });

        await db.SaveChangesAsync();

        return Results.Ok(new PlatformLoginResponse(
            accessToken,
            expiresAt,
            refreshTokenValue,
            BuildAuthenticatedUserResponse(user)));
    }

    /// <summary>
    /// Recarrega a sessão autenticada atual do sponsor a partir do banco.
    /// Isso garante que flags como troca obrigatória de senha reflitam o estado persistido.
    /// </summary>
    private static async Task<IResult> MeAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var platformUserId = GetPlatformUserId(user);
        if (platformUserId == Guid.Empty)
            return Results.Unauthorized();

        var platformUser = await db.PlatformUsers.FirstOrDefaultAsync(x => x.Id == platformUserId && x.IsActive);
        if (platformUser is null)
            return Results.Unauthorized();

        return Results.Ok(new PlatformAuthenticatedSessionResponse(
            platformUser.Id,
            platformUser.Name,
            platformUser.Email,
            platformUser.Role,
            platformUser.MustChangePassword,
            platformUser.ProfilePhotoUrl));
    }

    /// <summary>
    /// Renova a sessão do sponsor a partir de um refresh token ainda ativo.
    /// O token anterior é marcado como usado para impedir reaproveitamento indevido.
    /// </summary>
    private static async Task<IResult> RefreshAsync([FromBody] PlatformRefreshRequest request,
                                                    AppDbContext db,
                                                    IPlatformJwtService jwtService)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Results.BadRequest("Refresh token is required.");

        var refreshToken = await db.PlatformRefreshTokens.FirstOrDefaultAsync(x =>
            x.Token == request.RefreshToken &&
            !x.IsUsed &&
            !x.IsRevoked &&
            x.ExpiryDate > DateTime.UtcNow);

        if (refreshToken is null)
            return Results.Unauthorized();

        var user = await db.PlatformUsers.FirstOrDefaultAsync(x => x.Id == refreshToken.PlatformUserId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        var accessToken = jwtService.GenerateAccessToken(user, out var expiresAt);
        var nextRefreshTokenValue = jwtService.GenerateRefreshToken();

        refreshToken.IsUsed = true;

        db.PlatformRefreshTokens.Add(new PlatformRefreshToken
        {
            Id = Guid.NewGuid(),
            Token = nextRefreshTokenValue,
            PlatformUserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        });

        await db.SaveChangesAsync();

        return Results.Ok(new PlatformLoginResponse(
            accessToken,
            expiresAt,
            nextRefreshTokenValue,
            BuildAuthenticatedUserResponse(user)));
    }

    /// <summary>
    /// Altera a senha do sponsor e revoga as sessões ativas para forçar novo login nos demais clientes.
    /// Quando a troca ocorre com sucesso, a flag de troca obrigatória é removida.
    /// </summary>
    private static async Task<IResult> ChangePasswordAsync([FromBody] PlatformChangePasswordRequest request,
                                                           AppDbContext db,
                                                           IBCryptPasswordHasher passwordHasher,
                                                           ClaimsPrincipal user)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return Results.BadRequest("Current and new passwords are required.");

        if (request.NewPassword.Trim().Length < 6)
            return Results.BadRequest("New password must have at least 6 characters.");

        var platformUserId = GetPlatformUserId(user);
        if (platformUserId == Guid.Empty)
            return Results.Unauthorized();

        var platformUser = await db.PlatformUsers.FirstOrDefaultAsync(x => x.Id == platformUserId && x.IsActive);
        if (platformUser is null)
            return Results.Unauthorized();

        if (!passwordHasher.Verify(request.CurrentPassword, platformUser.PasswordHash))
            return Results.BadRequest("Current password is incorrect.");

        if (passwordHasher.Verify(request.NewPassword, platformUser.PasswordHash))
            return Results.BadRequest("New password must be different from current password.");

        platformUser.PasswordHash = passwordHasher.Hash(request.NewPassword);
        platformUser.MustChangePassword = false;

        var tokens = await db.PlatformRefreshTokens.Where(x => x.PlatformUserId == platformUserId && !x.IsRevoked).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await db.SaveChangesAsync();
        return Results.Ok("Password changed successfully and all active sessions were terminated.");
    }

    /// <summary>
    /// Revoga os refresh tokens ativos do sponsor para encerrar a sessão atual de forma segura.
    /// </summary>
    private static async Task<IResult> LogoutAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var platformUserId = GetPlatformUserId(user);
        if (platformUserId == Guid.Empty)
            return Results.Unauthorized();

        var tokens = await db.PlatformRefreshTokens.Where(x => x.PlatformUserId == platformUserId && !x.IsRevoked).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static PlatformAuthenticatedUserResponse BuildAuthenticatedUserResponse(PlatformUser user)
    {
        return new PlatformAuthenticatedUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.MustChangePassword,
            user.ProfilePhotoUrl);
    }

    private static Guid GetPlatformUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue("platform_user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
