using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
using FastBite.Core.Interfaces;

namespace FastBite.Implementation.Classes;
public class RedisService : IRedisService
{
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task SetAsync<T>(Guid key, T value)
    {
        string json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key.ToString(), json);
    }

    public async Task<T?> GetAsync<T>(Guid key)
    {
        string json = await _database.StringGetAsync(key.ToString());
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task<bool> DeleteAsync(Guid key)
    {
        return await _database.KeyDeleteAsync(key.ToString());
    }
}