namespace AlphaSquad.Api.Features.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        // Endpoint para recupera todos os usuários
        group.MapGet("/", GetAllAsync)
            .WithName("GetUsers")
            .Produces<List<UserResponse>>(StatusCodes.Status200OK);

        // Endpoint para recupera um usuário
        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetUserById")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Endpoint para criar usuário
        group.MapPost("/", CreateAsync)
            .WithName("CreateUser")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        // Endpoint para atualizar usuário
        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateUser")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        // Endpoint para remover um usuario
        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAllAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        // Por padrão, somente os usuários ativos são retornados
        var users = await db.Users
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new UserResponse
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Name = x.Name,
                Email = x.Email,
                Role = x.Role,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Results.Ok(users);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var user = await db.Users
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(x => new UserResponse
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Name = x.Name,
                Email = x.Email,
                Role = x.Role,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

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
            PasswordHash = passwordHasher.Hash(request.Password), // Hash da senha usando BCrypt
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var response = new UserResponse
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

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

        return Results.Ok(new UserResponse
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (user is null)
            return Results.NotFound();

        // Desativa o usuário em vez de deletar, para manter o histórico e evitar problemas de integridade referencial
        user.IsActive = false;
        
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}