namespace BuildingBlocks.Abstractions.Core;

public interface IExclusiveLock : IDisposable
{
    Task<object> AquireAsync(object obj, CancellationToken token = default);
    Task ReleaseAsync(object obj);
    void Execute<T>(T obj, Action<T> action, CancellationToken token = default);
    Task ExecuteAsync<T>(T obj, Func<T, Task> func, CancellationToken token = default);
}
