var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDependencies();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddEndpointsApiExplorer();

// Configura o Swagger para gerar a documentação da API, incluindo a definição de segurança para autenticação JWT.
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

// Configura o Entity Framework Core para usar o PostgreSQL como banco de dados, utilizando a string de conexão definida no appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configura as opções de JWT a partir da seção "Jwt" do appsettings.json, permitindo que sejam injetadas em outros serviços.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

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

// Configura as opções de Redis a partir da seção "Redis" do appsettings.json, permitindo que sejam injetadas em outros serviços.
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

//var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>()!;
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>()!;
    return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
});

// Configura as opções de armazenamento a partir da seção "Storage" do appsettings.json, permitindo que sejam injetadas em outros
// serviços relacionados ao armazenamento de arquivos, como o Cloudflare R2.
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

builder.Services.AddAuthorization();

var app = builder.Build();

// Ao iniciar a aplicação, executa o seeding do banco de dados para garantir que o tenant demo e o usuário admin existam
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
app.UseAuthorization();

app.MapControllers();

// Em vez de usar MapControllers, registramos os endpoints de autenticação, tenants e media diretamente,
// utilizando os métodos de extensão MapAuthEndpoints, MapTenantEndpoints e MapMediaEndpoints.
app.MapAuthEndpoints();
app.MapTenantEndpoints();
app.MapMediaEndpoints();
app.MapUserEndpoints();

app.Run();