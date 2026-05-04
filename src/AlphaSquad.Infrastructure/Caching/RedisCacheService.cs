namespace AlphaSquad.Infrastructure.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;
    private readonly string _instanceName;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> options)
    {
        _database = connectionMultiplexer.GetDatabase();
        _instanceName = options.Value.InstanceName;
    }

    // O método GetAsync é responsável por recuperar um valor do cache Redis usando uma chave fornecida.
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var redisKey = BuildKey(key);

        var value = await _database.StringGetAsync(redisKey);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value.ToString()!, JsonOptions);
    }

    // O método SetAsync é responsável por armazenar um valor no cache Redis com uma chave fornecida e um tempo de expiração.
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var redisKey = BuildKey(key);

        var serialized = JsonSerializer.Serialize(value, JsonOptions);

        await _database.StringSetAsync(redisKey, serialized, expiration);
    }

    // O método RemoveAsync é responsável por remover um valor do cache Redis usando uma chave fornecida.
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var redisKey = BuildKey(key);

        await _database.KeyDeleteAsync(redisKey);
    }

    private string BuildKey(string key)
    {
        return $"{_instanceName}{key}";
    }
}