namespace BuildingBlocks.Abstractions.Persistence;

/// <summary>
///     The unit of work pattern.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWork<out TContext> : IUnitOfWork
    where TContext : class
{
    TContext Context { get; }
}
