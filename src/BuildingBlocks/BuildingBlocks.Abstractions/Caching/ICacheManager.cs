namespace BuildingBlocks.Abstractions.Caching;

public interface ICacheManager
{
    ICacheProvider DefaultCacheProvider { get; }
}
