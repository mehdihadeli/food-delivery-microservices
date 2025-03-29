namespace BuildingBlocks.Core;

using System.Collections.Concurrent;
using BuildingBlocks.Abstractions.Core;
using Microsoft.Extensions.Logging;

public class ExclusiveLock(ILogger<ExclusiveLock> logger) : IExclusiveLock
{
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _semaphoreDictionary = new();
    private readonly ConcurrentDictionary<object, object> _lockDictionary = new();

    public Task<object> AquireAsync(object obj, CancellationToken token = default(CancellationToken))
    {
        var theLock = _lockDictionary.GetOrAdd(obj, o => new object());
        var semaphore = _semaphoreDictionary.GetOrAdd(theLock, o => new SemaphoreSlim(1, 1));

        return semaphore.WaitAsync(token).ContinueWith(t => theLock, token);
    }

    public Task ReleaseAsync(object obj)
    {
        var semaphore = _semaphoreDictionary.GetOrAdd(obj, o => new SemaphoreSlim(1, 1));
        semaphore.Release();
        return Task.FromResult(0);
    }

    public void Execute<T>(T obj, Action<T> action, CancellationToken token = default(CancellationToken))
    {
        var theLock = _lockDictionary.GetOrAdd(obj, o => new object());
        var semaphore = _semaphoreDictionary.GetOrAdd(theLock, o => new SemaphoreSlim(1, 1));
        semaphore.Wait(token);
        try
        {
            action(obj);
        }
        catch (System.Exception e)
        {
            logger.LogError("Exception when performing exclusive execute, {Error}", e);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task ExecuteAsync<T>(T obj, Func<T, Task> func, CancellationToken token = default(CancellationToken))
    {
        var theLock = _lockDictionary.GetOrAdd(obj, o => new object());
        var semaphore = _semaphoreDictionary.GetOrAdd(theLock, o => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(token);
        try
        {
            await func(obj);
        }
        catch (System.Exception e)
        {
            logger.LogError("Exception when performing exclusive execute async, {Error}", e);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void Dispose()
    {
        foreach (var slim in _semaphoreDictionary.Values)
        {
            slim.Dispose();
        }
    }
}
