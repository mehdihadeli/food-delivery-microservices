using BuildingBlocks.Abstractions.Caching;

namespace BuildingBlocks.Caching;

public class CacheOptions
{
    public double ExpirationTimeInMinute { get; set; } = 30;
    public double LocalCacheExpirationTimeInMinute { get; set; } = 5;
    public long MaximumPayloadBytes { get; set; } = 1024 * 1024;
    public int MaximumKeyLength { get; set; } = 1024;
    public CacheSerializationType SerializationType { get; set; } = CacheSerializationType.Json;
    public RedisDistributedCacheOptions RedisDistributedCacheOptions { get; set; } = new();
    public string DefaultCachePrefix { get; set; } = "Ch_";
}

public class RedisDistributedCacheOptions
{
    public string ConnectionString { get; set; } = default!;
    public bool DisableHealthChecks { get; set; }

    public bool DisableTracing { get; set; }
}
