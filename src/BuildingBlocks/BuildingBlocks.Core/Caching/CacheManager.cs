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
        DefaultCacheProvider = cacheProvider;
    }

    public ICacheProvider DefaultCacheProvider { get; }
}
