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

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized);

        return app;
    }

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