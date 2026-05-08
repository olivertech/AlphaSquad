namespace AlphaSquad.Api.Features.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("GetUsers")
            .WithSummary("Lista os usuários ativos do tenant atual.")
            .WithDescription("Retorna os usuários ativos vinculados ao tenant da sessão, ordenados por nome.")
            .Produces<List<UserResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
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
                             WHERE tenant_id = @TenantId AND is_active = true 
                             ORDER BY name";

        var users = await connection.QueryAsync<UserResponse>(sql, new { TenantId = tenantId });

        return Results.Ok(users);
    }

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
