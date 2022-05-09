namespace BuildingBlocks.Abstractions.Caching;

public interface ICacheManager
{
    /// <summary>
    /// Gets data asynchronously.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Gets data synchronously.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? Get<T>(string key);

    /// <summary>
    /// Sets data asynchronously.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value"></param>
    /// <param name="cacheTime">time in second format.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value, int? cacheTime = null)
        where T : notnull;

    /// <summary>
    /// Sets data asynchronously.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="acquire"></param>
    /// <param name="cacheTime">time in second format.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task SetAsync<T>(string key, Func<T> acquire, int? cacheTime = null)
        where T : notnull;

    /// <summary>
    /// Sets data synchronously.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value"></param>
    /// <param name="cacheTime">time in second format.</param>
    /// <typeparam name="T"></typeparam>
    void Set<T>(string key, T value, int? cacheTime = null)
        where T : notnull;

    /// <summary>
    /// Sets data synchronously.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="acquire"></param>
    /// <param name="cacheTime">time in second format.</param>
    /// <typeparam name="T"></typeparam>
    void Set<T>(string key, Func<T> acquire, int? cacheTime = null)
        where T : notnull;

    /// <summary>Gets or sets data asynchronously.</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">The key.</param>
    /// <param name="acquireAsync">The acquire asynchronous.</param>
    /// <param name="cacheTime">time in second format.</param>
    /// <returns></returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>>? acquireAsync = null, int? cacheTime = null);

    /// <summary>Gets or sets data.</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">The key.</param>
    /// <param name="acquire">The acquire.</param>
    /// <param name="cacheTime">time in second format.</param>
    /// <returns></returns>
    T? GetOrSet<T>(string key, Func<T>? acquire = null, int? cacheTime = null);

    /// <summary>Removes data from cache asynchronously.</summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    Task RemoveAsync(string key);

    /// <summary>Removes data from cache.</summary>
    /// <param name="key">The key.</param>
    void Remove(string key);
}
