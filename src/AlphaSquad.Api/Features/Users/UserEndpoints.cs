namespace AlphaSquad.Api.Features.Users;

public static class UserEndpoints
{
    /// <summary>
    /// Registra os endpoints administrativos de usuários do tenant.
    /// A leitura e a escrita ficam restritas a administradores por lidarem com cadastro, status e perfis de acesso.
    /// </summary>
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetUsers")
            .WithSummary("Lista os usuários do tenant atual.")
            .WithDescription("Retorna os usuários vinculados ao tenant da sessão, incluindo perfis ativos e inativos para uso administrativo.")
            .Produces<List<UserResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetUserById")
            .WithSummary("Busca um usuário específico do tenant atual.")
            .WithDescription("Retorna os dados de um usuário pelo identificador, respeitando o isolamento multi-tenant.")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateUser")
            .WithSummary("Cria um novo usuário no tenant atual.")
            .WithDescription("Cadastra um usuário com senha, role e vínculo ao tenant autenticado.")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateUser")
            .WithSummary("Atualiza um usuário do tenant atual.")
            .WithDescription("Permite alterar nome, role e status ativo de um usuário pertencente ao tenant da sessão.")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteUser")
            .WithSummary("Desativa um usuário do tenant atual.")
            .WithDescription("Realiza a exclusão lógica do usuário, marcando-o como inativo no banco.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Lista todos os usuários do tenant autenticado.
    /// A gestão precisa enxergar perfis ativos e inativos para conseguir auditar a base e reativar acessos quando necessário.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT id, 
                                    tenant_id AS TenantId, 
                                    name, 
                                    email, 
                                    role, 
                                    is_active AS IsActive, 
                                    created_at AS CreatedAt 
                             FROM users 
                             WHERE tenant_id = @TenantId
                             ORDER BY name";

        var users = await connection.QueryAsync<UserResponse>(sql, new { TenantId = tenantId });

        return Results.Ok(users);
    }

    /// <summary>
    /// Retorna um usuário específico do tenant atual.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT id, 
                                    tenant_id AS TenantId, 
                                    name, 
                                    email, 
                                    role, 
                                    is_active AS IsActive, 
                                    created_at AS CreatedAt 
                             FROM users 
                             WHERE id = @Id AND tenant_id = @TenantId";

        var user = await connection.QueryFirstOrDefaultAsync<UserResponse>(sql, new { Id = id, TenantId = tenantId });

        if (user is null)
            return Results.NotFound();

        return Results.Ok(user);
    }

    /// <summary>
    /// Cria um novo usuário no tenant autenticado.
    /// O e-mail é normalizado para comparação consistente e a senha já nasce com hash seguro.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateUserRequest request,
                                                   AppDbContext db,
                                                   IBCryptPasswordHasher passwordHasher,
                                                   HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return Results.BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest("Password is required.");

        var tenantId = context.GetTenantId();
        var email = request.Email.Trim().ToLower();

        var exists = await db.Users.AnyAsync(x => x.TenantId == tenantId && x.Email == email);

        if (exists)
            return Results.Conflict("A user with this e-mail already exists in this tenant.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var response = new UserResponse(
            user.Id,
            user.TenantId,
            user.Name,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAt
        );

        return Results.Created($"/api/users/{user.Id}", response);
    }

    /// <summary>
    /// Atualiza os dados administrativos de um usuário existente do tenant.
    /// Nesta V1 a equipe pode alterar nome, role e status ativo.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id,
                                                   UpdateUserRequest request,
                                                   AppDbContext db,
                                                   HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Name is required.");

        var tenantId = context.GetTenantId();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (user is null)
            return Results.NotFound();

        user.Name = request.Name.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        var response = new UserResponse(
            user.Id,
            user.TenantId,
            user.Name,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAt
        );

        return Results.Ok(response);
    }

    /// <summary>
    /// Desativa logicamente um usuário do tenant atual.
    /// O registro permanece no banco para preservar histórico e relacionamentos.
    /// </summary>
    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (user is null)
            return Results.NotFound();

        user.IsActive = false;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
