using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IsolationLevel = System.Data.IsolationLevel;

namespace BuildingBlocks.Core.Persistence.EfCore;

// https://github.com/Daniel127/EF-Unit-Of-Work
public class EfUnitOfWork<TDbContext>(
    TDbContext context,
    IDomainEventsAccessor domainEventsAccessor,
    IDomainEventPublisher domainEventPublisher,
    ILogger<EfUnitOfWork<TDbContext>> logger
) : IEfUnitOfWork<TDbContext>
    where TDbContext : EfDbContextBase
{
    public TDbContext DbContext => context;

    public DbSet<TEntity> Set<TEntity>()
        where TEntity : class
    {
        return context.Set<TEntity>();
    }

    public Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        return context.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return context.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = domainEventsAccessor.DequeueUncommittedDomainEvents();
        await domainEventPublisher.PublishAsync(domainEvents.ToArray(), cancellationToken);

        await context.CommitTransactionAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = domainEventsAccessor.DequeueUncommittedDomainEvents();
        await domainEventPublisher.PublishAsync(domainEvents.ToArray(), cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return context.RollbackTransactionAsync(cancellationToken);
    }

    public Task RetryOnExceptionAsync(Func<Task> operation)
    {
        return context.RetryOnExceptionAsync(operation);
    }

    public Task<TResult> RetryOnExceptionAsync<TResult>(Func<Task<TResult>> operation)
    {
        return context.RetryOnExceptionAsync(operation);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task ExecuteTransactionalAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        return context.ExecuteTransactionalAsync(action, cancellationToken);
    }

    public Task<T> ExecuteTransactionalAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        return context.ExecuteTransactionalAsync(action, cancellationToken);
    }
}
