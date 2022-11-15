namespace BuildingBlocks.Caching.Redis;

public interface IRedisCache
{
    Task<T?> GetAsync<T>(string key);
    T? Get<T>(string key);
    Task RemoveAsync(string key);
    void Remove(string key);
    Task SetAsync(string key, object data, int? cacheTime = null);
    void Set(string key, object data, int? cacheTime = null);
    Task<bool> IsSetAsync(string key);
    Task RemoveAllKeysAsync();
    Task<IEnumerable<string>> GetAllKeysAsync();
}
