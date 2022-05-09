namespace BuildingBlocks.Abstractions.Caching;

public interface ICacheProvider
{
    Task<T?> GetAsync<T>(string key);
    T? Get<T>(string key);
    Task SetAsync(string key, object data, int? cacheTime = null);
    void Set(string key, object data, int? cacheTime = null);
    Task<bool> IsSetAsync(string key);
    bool IsSet(string key);
    Task RemoveAsync(string key);
    void Remove(string key);
}
