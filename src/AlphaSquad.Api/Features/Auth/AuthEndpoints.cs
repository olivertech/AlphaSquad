namespace AlphaSquad.Api.Features.Auth;

public static class AuthEndpoints
{
    /// <summary>
    /// O MapAuthEndpoints define os endpoints relacionados à autenticação, como login. 
    /// Ele é usado para organizar e agrupar as rotas de autenticação sob um prefixo 
    /// comum (/api/auth) e aplicar tags para documentação.
    /// Com essa classe e método, o código de configuração dos endpoints de autenticação 
    /// fica centralizado e fácil de manter, além de melhorar a clareza e a organização do código da API.
    /// No Program.cs, o método MapAuthEndpoints é chamado para registrar esses endpoints na aplicação, 
    /// ao invés de chamar MapControllers, garantindo que as rotas de autenticação estejam disponíveis para os clientes da API.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        // Endpoint de login, que é público (AllowAnonymous) e retorna um token JWT em caso de sucesso.
        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized);

        // Endpoint para obter informações do usuário autenticado (me), que requer autenticação (RequireAuthorization).
        group.MapGet("/me", MeAsync)
            .RequireAuthorization()
            .WithName("Me")
            .Produces<AuthenticatedUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // Endpoint para renovar o Access Token utilizando um Refresh Token válido.
        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("Refresh")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized);

        // Endpoint para alteração de senha do usuário autenticado.
        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        // Endpoint para encerrar a sessão, revogando todos os Refresh Tokens do usuário.
        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);
        
        return app;
    }

    /// <summary>
    /// Endpoint para obter informações do usuário autenticado (me), que requer autenticação (RequireAuthorization).
    /// </summary>
    /// <param name="user">O usuário autenticado.</param>
    /// <returns>As informações do usuário autenticado.</returns>
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

    /// <summary>
    /// Endpoint de login, que é público (AllowAnonymous) e retorna um token JWT em caso de sucesso.
    /// Implementa a verificação de credenciais e a geração do par Access Token e Refresh Token.
    /// </summary>
    /// <param name="request">O objeto de requisição de login.</param>
    /// <param name="db">O contexto do banco de dados.</param>
    /// <param name="passwordHasher">O serviço de hash de senhas.</param>
    /// <param name="jwtService">O serviço de geração de tokens JWT.</param>
    /// <returns>O resultado da operação de login.</returns>
    private static async Task<IResult> LoginAsync([FromBody] LoginRequest request,
                                                  AppDbContext db,
                                                  IBCryptPasswordHasher passwordHasher,
                                                  IJwtService jwtService)
    {
        if (string.IsNullOrWhiteSpace(request.TenantSlug) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest("Tenant, e-mail and password are required.");
        }

        var tenantSlug = request.TenantSlug.Trim().ToLower();
        var email = request.Email.Trim();

        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == tenantSlug && x.IsActive);

        if (tenant is null)
            return Results.Unauthorized();

        var user = await db.Users
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenant.Id &&
                x.Email == email &&
                x.IsActive);

        if (user is null)
            return Results.Unauthorized();

        var passwordValid = passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
            return Results.Unauthorized();

        var accessToken = jwtService.GenerateAccessToken(user, tenant, out var expiresAt);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Persiste o Refresh Token no banco para controle de sessão e revogação.
        var refreshCid = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        db.RefreshTokens.Add(refreshCid);
        await db.SaveChangesAsync();

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            User = new AuthenticatedUserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            },
            Tenant = new AuthenticatedTenantResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                LogoUrl = tenant.LogoUrl,
                PrimaryColor = tenant.PrimaryColor,
                SecondaryColor = tenant.SecondaryColor
            }
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Endpoint para renovar o Access Token. 
    /// Implementa a estratégia de rotação de tokens: o Refresh Token utilizado é marcado como usado,
    /// e um novo par (Access + Refresh) é gerado, prevenindo ataques de replay.
    /// </summary>
    /// <param name="request">Objeto contendo o Refresh Token a ser validado.</param>
    /// <param name="db">O contexto do banco de dados.</param>
    /// <param name="jwtService">O serviço de geração de tokens.</param>
    /// <returns>Um novo conjunto de tokens e informações do usuário/tenant.</returns>
    private static async Task<IResult> RefreshAsync([FromBody] RefreshRequest request,
                                                   AppDbContext db,
                                                   IJwtService jwtService)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Results.BadRequest("Refresh token is required.");

        // Busca o token e inclui as entidades relacionadas para gerar o novo Access Token.
        var refreshToken = await db.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(u => u.Tenant)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (refreshToken is null || !refreshToken.IsActive)
            return Results.Unauthorized();

        var user = refreshToken.User;
        var tenant = user.Tenant;

        var accessToken = jwtService.GenerateAccessToken(user, tenant, out var expiresAt);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        // Marca o token atual como usado para evitar reuso.
        refreshToken.IsUsed = true;

        // Gera um novo Refresh Token para a próxima renovação.
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

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = newRefreshToken,
            User = new AuthenticatedUserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            },
            Tenant = new AuthenticatedTenantResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                LogoUrl = tenant.LogoUrl,
                PrimaryColor = tenant.PrimaryColor,
                SecondaryColor = tenant.SecondaryColor
            }
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Endpoint para alteração de senha.
    /// Valida a senha atual antes de permitir a atualização para garantir que o usuário está autenticado.
    /// Após a troca, revoga todos os tokens de atualização para forçar um novo login em todos os dispositivos.
    /// </summary>
    /// <param name="request">Objeto com a senha atual e a nova senha.</param>
    /// <param name="db">O contexto do banco de dados.</param>
    /// <param name="passwordHasher">O serviço de hash de senhas.</param>
    /// <param name="user">O principal do usuário autenticado.</param>
    /// <returns>Mensagem de sucesso ou erro.</returns>
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

        // Valida a senha atual.
        if (!passwordHasher.Verify(request.CurrentPassword, appUser.PasswordHash))
            return Results.BadRequest("Current password is incorrect.");

        // Atualiza para a nova senha com novo hash.
        appUser.PasswordHash = passwordHasher.Hash(request.NewPassword);
        
        // Por segurança, revoga todos os Refresh Tokens ativos do usuário ao alterar a senha.
        // Isso impede que sessões antigas continuem ativas após a troca de senha.
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await db.SaveChangesAsync();

        return Results.Ok("Password changed successfully and all active sessions were terminated.");
    }

    /// <summary>
    /// Endpoint de logout.
    /// Revoga todos os Refresh Tokens ativos do usuário para garantir que nenhuma sessão antiga possa ser usada.
    /// </summary>
    /// <param name="db">O contexto do banco de dados.</param>
    /// <param name="user">O principal do usuário autenticado.</param>
    /// <returns>NoContent em caso de sucesso.</returns>
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
