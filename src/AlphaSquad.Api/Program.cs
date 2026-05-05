var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços necessários para a aplicação, incluindo controladores, dependências personalizadas, 
// e configuração do Swagger para documentação da API.
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

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configura as opções de JWT a partir da seção "Jwt" do appsettings.json, permitindo que sejam injetadas em outros serviços.
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

var jwtOptions = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtOptions>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection("Redis"));

var redisOptions = builder.Configuration
    .GetSection("Redis")
    .Get<RedisOptions>()!;

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
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

// Em vez de usar MapControllers, registramos os endpoints de autenticação e tenants diretamente,
app.MapAuthEndpoints();
app.MapTenantEndpoints();

app.Run();