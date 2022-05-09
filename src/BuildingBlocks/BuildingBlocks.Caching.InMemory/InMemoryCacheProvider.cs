using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Caching.InMemory;

public class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly InMemoryCacheOptions _options;

    public InMemoryCacheProvider(IMemoryCache memoryCache, IOptions<InMemoryCacheOptions> cacheOptions)
    {
        _memoryCache = Guard.Against.Null(memoryCache, nameof(memoryCache));
        _options = cacheOptions.Value;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));

        await Task.CompletedTask;
        return Get<T>(key);
    }

    public void Set(string key, object data, int? cacheTime = null)
    {
        Guard.Against.Null(data, nameof(data));
        Guard.Against.NullOrEmpty(key, nameof(key));

        var memoryCacheEntryOptions =
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                TimeSpan.FromSeconds(cacheTime ?? _options.DefaultCacheTime));

        _memoryCache.Set(key, data, memoryCacheEntryOptions);
    }

    public async Task<bool> IsSetAsync(string key)
    {
        await Task.CompletedTask;
        return IsSet(key);
    }

    public async Task RemoveAsync(string key)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));
        await Task.CompletedTask;

        Remove(key);
    }

    public T Get<T>(string key)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));

        return (T)_memoryCache.Get(key);
    }

    public async Task SetAsync(string key, object data, int? cacheTime = null)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));
        Guard.Against.Null(data, nameof(data));

        await Task.CompletedTask;
        Set(key, data, cacheTime ?? _options.DefaultCacheTime);
    }

    public bool IsSet(string key)
    {
        return _memoryCache.Get(key) != null;
    }

    public void Remove(string key)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));

        _memoryCache.Remove(key);
    }
}
