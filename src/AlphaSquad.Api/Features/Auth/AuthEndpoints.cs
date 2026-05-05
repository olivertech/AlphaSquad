using System.Security.Claims;

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

        var token = jwtService.GenerateAccessToken(user, tenant, out var expiresAt);

        var response = new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
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
}