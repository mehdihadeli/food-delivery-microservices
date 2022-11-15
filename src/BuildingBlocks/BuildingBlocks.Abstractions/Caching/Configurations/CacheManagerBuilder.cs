namespace BuildingBlocks.Abstractions.Caching;

public class CacheManagerBuilder : ICacheManagerBuilder
{
    private readonly CacheManagerConfiguration _cacheManagerConfiguration;

    public CacheManagerBuilder AddRedisProvider(
        string providerName,
        Action<RedisConfigurationBuilder> redisConfigurationBuilder)
    {
        return this;
    }

    public CacheManagerBuilder AddInMemoryProvider(
        string providerName,
        Action<InMemoryConfigurationBuilder> inMemoryConfigurationBuilder)
    {
        return this;
    }
}
