using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Caching;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Caching;

public class CacheManager : ICacheManager
{
    private readonly ICacheProvider _cacheProvider;
    private readonly CacheOptions _options;

    public CacheManager(ICacheProvider cacheProvider, IOptions<CacheOptions> options)
    {
        _cacheProvider = cacheProvider;
        _options = options.Value;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return _cacheProvider.GetAsync<T>(key);
    }

    public T? Get<T>(string key)
    {
        return _cacheProvider.Get<T>(key);
    }

    public Task SetAsync<T>(string key, T value, int? cacheTime = null)
        where T : notnull
    {
        return _cacheProvider.SetAsync(key, value, cacheTime ?? _options.DefaultCacheTime);
    }

    public Task SetAsync<T>(string key, Func<T> acquire, int? cacheTime = null)
        where T : notnull
    {
        var result = acquire();

        _cacheProvider.Set(key, result, cacheTime ?? _options.DefaultCacheTime);

        return Task.CompletedTask;
    }

    public void Set<T>(string key, T value, int? cacheTime = null)
        where T : notnull
    {
        _cacheProvider.Set(key, value, cacheTime ?? _options.DefaultCacheTime);
    }

    public void Set<T>(string key, Func<T> acquire, int? cacheTime = null)
        where T : notnull
    {
        var result = acquire();

        Guard.Against.Null(result, nameof(result));

        _cacheProvider.Set(key, result, cacheTime ?? _options.DefaultCacheTime);
    }

    /// <inheritdoc cref="" />
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>>? acquireAsync = null, int? cacheTime = null)
    {
        var data = await _cacheProvider.GetAsync<T>(key);

        if (data != null)
        {
            return data;
        }

        if (acquireAsync is { })
        {
            var result = await acquireAsync();

            Guard.Against.Null(result, nameof(result));

            await _cacheProvider.SetAsync(key, result, cacheTime ?? _options.DefaultCacheTime);

            return result;
        }

        return default;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key)
    {
        return _cacheProvider.RemoveAsync(key);
    }

    /// <inheritdoc cref="" />
    public T? GetOrSet<T>(string key, Func<T>? acquire = null, int? cacheTime = null)
    {
        var data = _cacheProvider.Get<T>(key);

        if (data != null)
        {
            return data;
        }

        if (acquire is { })
        {
            var result = acquire();

            Guard.Against.Null(result, nameof(result));

            _cacheProvider.Set(key, result, cacheTime ?? _options.DefaultCacheTime);

            return result;
        }

        return default;
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        _cacheProvider.Remove(key);
    }
}
