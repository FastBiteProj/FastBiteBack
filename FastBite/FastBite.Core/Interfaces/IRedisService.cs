namespace FastBite.Core.Interfaces;

public interface IRedisService
{
    public Task SetAsync<T>(Guid key, T value);
    public Task<T?> GetAsync<T>(Guid key);
    public Task<bool> DeleteAsync(Guid key);
}