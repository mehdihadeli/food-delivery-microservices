namespace BuildingBlocks.Abstractions.Caching;

/// <summary>
/// Interface defining a contract for a caching service to handle common caching operations,
/// including retrieval, insertion, existence checks, and removal of cache entries,
/// with support for hybrid (local and distributed) caching.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value stored in the cache.</typeparam>
    /// <param name="key">The unique key identifying the cache entry.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the cached value or <c>null</c> if the key does not exist.
    /// </returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a cached value if it exists; otherwise, generates and stores the value using the provided factory function.
    /// Allows customization of cache expiration policies.
    /// </summary>
    /// <typeparam name="T">The type of the value stored in the cache.</typeparam>
    /// <param name="key">The unique key identifying the cache entry.</param>
    /// <param name="factory">
    /// A function that generates the value to be cached if it does not already exist.
    /// The function takes a <see cref="CancellationToken"/> and returns a <see cref="ValueTask{T}"/>.
    /// </param>
    /// <param name="tags">
    /// Optional tags to associate with the cache entry for grouping or selective removal.
    /// </param>
    /// <param name="localExpiration">
    /// Optional expiration time for the local (in-memory) cache. If not specified, the default value from configuration is used.
    /// </param>
    /// <param name="distributedExpiration">
    /// Optional expiration time for the distributed cache. If not specified, the default value from configuration is used.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the cached or newly generated value.
    /// </returns>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        IEnumerable<string>? tags = null,
        TimeSpan? localExpiration = null,
        TimeSpan? distributedExpiration = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds or updates a cache entry with the specified key and value.
    /// Allows customization of cache expiration policies.
    /// </summary>
    /// <typeparam name="T">The type of the value to be stored in the cache.</typeparam>
    /// <param name="key">The unique key identifying the cache entry.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="tags">
    /// Optional tags to associate with the cache entry for grouping or selective removal.
    /// </param>
    /// <param name="localExpiration">
    /// Optional expiration time for the local (in-memory) cache. If not specified, the default value from configuration is used.
    /// </param>
    /// <param name="distributedExpiration">
    /// Optional expiration time for the distributed cache. If not specified, the default value from configuration is used.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task SetAsync<T>(
        string key,
        T value,
        IEnumerable<string>? tags = null,
        TimeSpan? localExpiration = null,
        TimeSpan? distributedExpiration = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks whether a cache entry with the specified key exists.
    /// </summary>
    /// <param name="key">The unique key identifying the cache entry.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result is <c>true</c> if the cache entry exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cache entry with the specified key.
    /// </summary>
    /// <param name="key">The unique key identifying the cache entry to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries associated with the specified tags.
    /// </summary>
    /// <param name="tags">A collection of tags identifying the cache entries to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
}
