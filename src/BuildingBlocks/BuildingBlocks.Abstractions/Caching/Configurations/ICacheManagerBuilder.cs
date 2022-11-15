namespace BuildingBlocks.Abstractions.Caching;

public interface ICacheManagerBuilder
{
    CacheManagerBuilder AddRedisProvider(
        string providerName,
        Action<RedisConfigurationBuilder> redisConfigurationBuilder);

    CacheManagerBuilder AddInMemoryProvider(
        string providerName,
        Action<InMemoryConfigurationBuilder> inMemoryConfigurationBuilder);
}
