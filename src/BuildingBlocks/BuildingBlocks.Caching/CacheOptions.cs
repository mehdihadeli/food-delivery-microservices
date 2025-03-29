using BuildingBlocks.Abstractions.Caching;

namespace BuildingBlocks.Caching;

public class CacheOptions
{
    public bool UseRedisDistributedCache { get; set; }
    public double ExpirationTimeInMinute { get; set; } = 30;
    public double LocalCacheExpirationTimeInMinute { get; set; } = 5;
    public long MaximumPayloadBytes { get; set; } = 1024 * 1024;
    public int MaximumKeyLength { get; set; } = 1024;
    public CacheSerializationType SerializationType { get; set; } = CacheSerializationType.Json;
    public RedisDistributedCacheOptions? RedisCacheOptions { get; set; } = default!;
    public string DefaultCachePrefix { get; set; } = "Ch_";
}

public class RedisDistributedCacheOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool AllowAdmin { get; set; }
}
