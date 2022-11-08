using System.Collections.Immutable;
using System.Data;
using System.Linq.Expressions;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Core.Persistence.EfCore;

public abstract class EfDbContextBase :
    DbContext,
    IDbFacadeResolver,
    IDbContext,
    IDomainEventContext
{
    // private readonly IDomainEventPublisher _domainEventPublisher;
    private IDbContextTransaction? _currentTransaction;

    protected EfDbContextBase(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        AddingSofDeletes(modelBuilder);
        AddingVersioning(modelBuilder);
    }

    private static void AddingVersioning(ModelBuilder builder)
    {
        var types = builder.Model.GetEntityTypes().Where(x => x.ClrType.IsAssignableTo(typeof(IHaveAggregateVersion)));
        foreach (var entityType in types)
        {
            builder.Entity(entityType.ClrType).Property(nameof(IHaveAggregateVersion.OriginalVersion))
                .IsConcurrencyToken();
        }
    }

    // Ref: https://www.meziantou.net/entity-framework-core-soft-delete-using-query-filters.htm
    private static void AddingSofDeletes(ModelBuilder builder)
    {
        var types = builder.Model.GetEntityTypes().Where(x => x.ClrType.IsAssignableTo(typeof(IHaveSoftDelete)));
        foreach (var entityType in types)
        {
            // 1. Add the IsDeleted property
            entityType.AddProperty("IsDeleted", typeof(bool));

            // 2. Create the query filter
            var parameter = Expression.Parameter(entityType.ClrType);

            // EF.Property<bool>(TEntity, "IsDeleted")
            var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
            var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));

            // EF.Property<bool>(TEntity, "IsDeleted") == false
            BinaryExpression compareExpression =
                Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));

            // TEntity => EF.Property<bool>(TEntity, "IsDeleted") == false
            var lambda = Expression.Lambda(compareExpression, parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        _currentTransaction ??= await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task RetryOnExceptionAsync(Func<Task> operation)
    {
        return Database.CreateExecutionStrategy().ExecuteAsync(operation);
    }

    public Task<TResult> RetryOnExceptionAsync<TResult>(Func<Task<TResult>> operation)
    {
        return Database.CreateExecutionStrategy().ExecuteAsync(operation);
    }

    public IReadOnlyList<IDomainEvent> GetAllUncommittedEvents()
    {
        var domainEvents = ChangeTracker
            .Entries<IHaveAggregate>()
            .Where(x => x.Entity.GetUncommittedDomainEvents().Any())
            .SelectMany(x => x.Entity.GetUncommittedDomainEvents())
            .ToList();

        return domainEvents.ToImmutableList();
    }

    public void MarkUncommittedDomainEventAsCommitted()
    {
        ChangeTracker.Entries<IHaveAggregate>()
            .Where(x => x.Entity.GetUncommittedDomainEvents().Any())
            .ToList()
            .ForEach(x => x.Entity.MarkUncommittedDomainEventAsCommitted());
    }

    public Task ExecuteTransactionalAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database
                .BeginTransactionAsync(cancellationToken);
            try
            {
                await action();

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public Task<T> ExecuteTransactionalAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database
                .BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await action();

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
