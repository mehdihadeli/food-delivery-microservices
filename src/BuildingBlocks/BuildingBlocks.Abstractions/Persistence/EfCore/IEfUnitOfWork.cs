using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Abstractions.Persistence.EfCore;

public interface IEfUnitOfWork : IUnitOfWork, ITransactionAble, ITxDbContextExecution, IRetryDbContextExecution
{
}

/// <summary>
/// Defines the interface(s) for generic unit of work.
/// </summary>
public interface IEfUnitOfWork<out TContext> : IEfUnitOfWork
    where TContext : DbContext
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    /// <returns>The instance of type <typeparamref name="TContext"/>.</returns>
    TContext DbContext { get; }
}
