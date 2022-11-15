using System.Text;
using BuildingBlocks.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BuildingBlocks.Caching.Redis;

public class RedisCacheProvider : ICacheProvider
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _config;

    public RedisCacheProvider(IDistributedCache cache, IConfiguration config)
    {
        _cache = cache;
        _config = config;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken token = default)
    {
        return await GetInternalAsync<T>(key, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync(key, null, null, null, factory, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        TimeSpan slidingExpiration,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync(key, slidingExpiration, null, null, factory, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        DateTime absoluteExpiration,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync(key, null, absoluteExpiration, null, factory, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        TimeSpan slidingExpiration,
        DateTime absoluteExpiration,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync(key, slidingExpiration, absoluteExpiration, null, factory, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        TimeSpan slidingExpiration,
        TimeSpan absoluteExpirationRelativeToNow,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync(key, slidingExpiration, null, absoluteExpirationRelativeToNow, factory,
            token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        TimeSpan? slidingExpiration,
        DateTime? absoluteExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync(
            key,
            slidingExpiration,
            absoluteExpiration,
            absoluteExpirationRelativeToNow,
            factory,
            token);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken token = default)
    {
        await SetInternalAsync(key, value, null, null, null, token);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        DateTime absoluteExpiration,
        CancellationToken token = default)
    {
        await SetInternalAsync(key, value, null, absoluteExpiration, null, token);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan slidingExpiration,
        CancellationToken token = default)
    {
        await SetInternalAsync(key, value, slidingExpiration, null, null, token);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration,
        DateTime? absoluteExpiration,
        CancellationToken token = default)
    {
        await SetInternalAsync(key, value, slidingExpiration, absoluteExpiration, null, token);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        CancellationToken token = default)
    {
        await SetInternalAsync(key, value, slidingExpiration, null, absoluteExpirationRelativeToNow, token);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration,
        DateTime? absoluteExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        CancellationToken token = default)
    {
        await SetInternalAsync(
            key,
            value,
            slidingExpiration,
            absoluteExpiration,
            absoluteExpirationRelativeToNow,
            token);
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        await _cache.RefreshAsync(key, token);
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        await _cache.RemoveAsync(key, token);
    }

    private async Task<T?> GetInternalAsync<T>(string key, CancellationToken token = default)
    {
        var bytes = await _cache.GetAsync(key, token);
        if (bytes is null)
            return default;

        var jsonString = Encoding.UTF8.GetString(bytes);

        return JsonConvert.DeserializeObject<T>(jsonString);
    }

    private async Task<T?> GetOrCreateInternalAsync<T>(
        string key,
        TimeSpan? slidingExpiration,
        DateTime? absoluteExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        Func<Task<T>> factory,
        CancellationToken token = default)
    {
        var value = await GetInternalAsync<T>(key, token);

        if (value != null) return value;

        value = await factory();

        if (value != null)
        {
            await SetInternalAsync<T>(
                key,
                value,
                slidingExpiration,
                absoluteExpiration,
                absoluteExpirationRelativeToNow,
                token);
        }

        return value;
    }

    private async Task SetInternalAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration,
        DateTime? absoluteExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        CancellationToken token = default)
    {
        var cacheEntryOptions = new DistributedCacheEntryOptions();
        if (slidingExpiration.HasValue)
        {
            cacheEntryOptions.SlidingExpiration = slidingExpiration.Value;
        }

        if (absoluteExpiration.HasValue)
        {
            cacheEntryOptions.AbsoluteExpiration = absoluteExpiration.Value;
        }

        if (absoluteExpirationRelativeToNow.HasValue)
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow.Value;
        }

        if (!slidingExpiration.HasValue && !absoluteExpiration.HasValue && !absoluteExpirationRelativeToNow.HasValue)
        {
            cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromSeconds(30));
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        }

        var jsonString = JsonConvert.SerializeObject(value);
        var bytes = Encoding.UTF8.GetBytes(jsonString);
        await _cache.SetAsync(key, bytes, cacheEntryOptions, token);
    }
}
