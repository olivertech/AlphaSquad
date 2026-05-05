namespace AlphaSquad.Api.DependencyInjections;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registra os serviços personalizados da aplicação, como o serviço de cache Redis, 
    /// o serviço de JWT e o hasher de senhas BCrypt, além do seeding do banco de dados.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IBCryptPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
