namespace BuildingBlocks.Core.Caching;

public class CacheOptions
{
    /// <summary>
    /// Gets or sets the value indicating the default cache time (in seconds).
    /// Default value is 60.
    /// </summary>
    public int DefaultCacheTime { get; set; } = 60;
}
