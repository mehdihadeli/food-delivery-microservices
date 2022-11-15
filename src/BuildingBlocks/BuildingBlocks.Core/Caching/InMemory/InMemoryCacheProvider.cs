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

    public Task<T?> GetAsync<T>(string key, CancellationToken token = default)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));

        return Task.FromResult(_memoryCache.Get<T>(key));
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync<T>(key, null, null, null, factory!, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        TimeSpan slidingExpiration,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync<T>(key, slidingExpiration, null, null, factory!, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        DateTime absoluteExpiration,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync<T>(key, null, absoluteExpiration, null, factory!, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        TimeSpan slidingExpiration,
        DateTime absoluteExpiration,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync<T>(key, slidingExpiration, absoluteExpiration, null, factory!, token);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        TimeSpan? slidingExpiration,
        DateTime? absoluteExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        Func<Task<T?>> factory,
        CancellationToken token = default)
    {
        return await GetOrCreateInternalAsync<T>(
            key,
            slidingExpiration,
            absoluteExpiration,
            absoluteExpirationRelativeToNow,
            factory!,
            token);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken token = default)
    {
        await SetInternalAsync(key, value, null, null, null, token);
    }

    public async Task SetAsync<T>(string key, T value, DateTime absoluteExpiration, CancellationToken token = default)
    {
        await SetInternalAsync(key, value, null, absoluteExpiration, null, token);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration, CancellationToken token = default)
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

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    private async Task<T?> GetOrCreateInternalAsync<T>(
        string key,
        TimeSpan? slidingExpiration,
        DateTime? absoluteExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        Func<Task<T>> factory,
        CancellationToken token = default)
    {
        var val = await _memoryCache.GetOrCreateAsync(key, async f =>
        {
            f.SlidingExpiration = slidingExpiration;
            f.AbsoluteExpiration = absoluteExpiration;
            f.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            return await factory();
        });

        // var value = _memoryCache.Get<T>(key);
        //
        // if (value != null) return value;
        //
        // value = await factory();
        //
        // if (value != null)
        // {
        //     await SetInternalAsync<T>(
        //         key,
        //         value,
        //         slidingExpiration,
        //         absoluteExpiration,
        //         absoluteExpirationRelativeToNow,
        //         token);
        // }
        return val;
    }


    private Task SetInternalAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration,
        DateTime? absoluteExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        CancellationToken token = default)
    {
        var memoryCacheEntryOptions = new MemoryCacheEntryOptions();
        if (slidingExpiration.HasValue)
        {
            memoryCacheEntryOptions.SlidingExpiration = slidingExpiration.Value;
        }

        if (absoluteExpiration.HasValue)
        {
            memoryCacheEntryOptions.AbsoluteExpiration = absoluteExpiration.Value;
        }

        if (absoluteExpirationRelativeToNow.HasValue)
        {
            memoryCacheEntryOptions.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow.Value;
        }

        if (!slidingExpiration.HasValue && !absoluteExpiration.HasValue && !absoluteExpirationRelativeToNow.HasValue)
        {
            memoryCacheEntryOptions.SetSlidingExpiration(TimeSpan.FromSeconds(30));
            memoryCacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        }

        _memoryCache.Set(key, value, memoryCacheEntryOptions);

        return Task.CompletedTask;
    }
}
