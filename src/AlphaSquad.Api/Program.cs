var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDependencies();
builder.Services.AddEndpointsApiExplorer();

// Configura o Swagger para gerar a documentaÃ§Ã£o da API, incluindo a definiÃ§Ã£o de seguranÃ§a para autenticaÃ§Ã£o JWT.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlphaSquad API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// Configura o Entity Framework Core para usar o PostgreSQL como banco de dados, utilizando a string de conexÃ£o definida no appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configura as opÃ§Ãµes de JWT a partir da configuracao central da aplicacao.
// Na V1, os valores podem vir de appsettings, user-secrets ou variaveis de ambiente.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<PlatformBootstrapOptions>(builder.Configuration.GetSection("PlatformBootstrap"));

//var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Configura as opcoes de Redis a partir da configuracao central da aplicacao.
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

//var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>()!;
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>()!;
    return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
});

// Configura as opcoes de armazenamento a partir da configuracao central.
// O recomendado e manter segredos fora do appsettings versionado, usando user-secrets ou variaveis de ambiente.
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var options = config.GetSection("Storage").Get<StorageOptions>();

    var s3Config = new AmazonS3Config
    {
        ServiceURL = $"https://{options!.Endpoint}",
        ForcePathStyle = true
    };

    return new AmazonS3Client(
        options.AccessKey,
        options.SecretKey,
        s3Config
    );
});

builder.Services.AddAuthorization(options =>
{
    // Policy base para acoes exclusivas de administracao do tenant.
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(nameof(UserRole.Admin)));

    // Policy base para operacoes gerenciais compartilhadas entre administradores e professores.
    options.AddPolicy(AuthorizationPolicies.AdminOrTeacher, policy =>
        policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Teacher)));

    // Policy exclusiva do backoffice master, separando a gestao da AlphaSquad do contexto das academias.
    options.AddPolicy(AuthorizationPolicies.PlatformOwnerOnly, policy =>
        policy.RequireClaim("platform_scope", PlatformClaimValues.Owner));
});

var app = builder.Build();

// Ao iniciar a aplicaÃ§Ã£o, executa o seeding do banco de dados para garantir que o tenant demo e o usuÃ¡rio admin existam
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

// Em vez de usar MapControllers, registramos os endpoints de autenticaÃ§Ã£o, tenants e media diretamente,
// utilizando os mÃ©todos de extensÃ£o MapAuthEndpoints, MapTenantEndpoints e MapMediaEndpoints.
app.MapAuthEndpoints();
app.MapTenantEndpoints();
app.MapMediaEndpoints();
app.MapUserEndpoints();
app.MapClassEndpoints();
app.MapExerciseEndpoints();
app.MapWorkoutEndpoints();
app.MapCheckInEndpoints();
app.MapProfileEndpoints();
app.MapPlanEndpoints();
app.MapStoreEndpoints();
app.MapSocialEndpoints();
app.MapEventEndpoints();
app.MapGamificationEndpoints();
app.MapLegalEndpoints();
app.MapConfigurationEndpoints();
app.MapNotificationEndpoints();
app.MapPlatformAuthEndpoints();
app.MapPlatformProfileEndpoints();
app.MapPlatformTenantEndpoints();

app.Run();
