using BuildingBlocks.Core.Caching;

namespace BuildingBlocks.Caching.Redis;

public class RedisCacheOptions : CacheOptions
{
    public string ConnectionString { get; set; } = null!;
    public int Db { get; set; } = -1;
    public object? AsyncState { get; set; }
}
