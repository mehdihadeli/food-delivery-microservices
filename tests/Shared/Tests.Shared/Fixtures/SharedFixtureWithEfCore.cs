using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Fixtures;

public class SharedFixtureWithEfCore<TEntryPoint, TEfCoreDbContext> : SharedFixture<TEntryPoint>
    where TEfCoreDbContext : DbContext
    where TEntryPoint : class
{
    public async Task ExecuteTxContextAsync(Func<IServiceProvider, TEfCoreDbContext, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TEfCoreDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task ExecuteAndResetStateContextAsync(Func<IServiceProvider, TEfCoreDbContext, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TEfCoreDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.RollbackTransactionAsync();
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task<T> ExecuteTxContextAsync<T>(Func<IServiceProvider, TEfCoreDbContext, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TEfCoreDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task<T> ExecuteAndResetStateContextAsync<T>(Func<IServiceProvider, TEfCoreDbContext, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TEfCoreDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.RollbackTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task ExecuteEfDbContextAsync(Func<IServiceProvider, TEfCoreDbContext, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        await ExecuteScopeAsync(sp => action(scope.ServiceProvider, sp.GetRequiredService<TEfCoreDbContext>()));
    }

    public async Task<T> ExecuteEfDbContextAsync<T>(Func<IServiceProvider, TEfCoreDbContext, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        return await ExecuteScopeAsync(sp => action(scope.ServiceProvider, sp.GetRequiredService<TEfCoreDbContext>()));
    }

    public Task ExecuteEfDbContextAsync(Func<TEfCoreDbContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TEfCoreDbContext>()));

    public Task ExecuteEfDbContextAsync(Func<TEfCoreDbContext, IMediator, Task> action)
        => ExecuteScopeAsync(
            sp => action(sp.GetRequiredService<TEfCoreDbContext>(), sp.GetRequiredService<IMediator>()));

    public Task<T> ExecuteEfDbContextAsync<T>(Func<TEfCoreDbContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TEfCoreDbContext>()));

    public Task<T> ExecuteEfDbContextAsync<T>(Func<TEfCoreDbContext, IMediator, Task<T>> action)
        => ExecuteScopeAsync(
            sp => action(sp.GetRequiredService<TEfCoreDbContext>(), sp.GetRequiredService<IMediator>()));

    public Task ExecuteEfDbContextAsync(Func<TEfCoreDbContext, ICommandProcessor, Task> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TEfCoreDbContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task<T> ExecuteEfDbContextAsync<T>(Func<TEfCoreDbContext, ICommandProcessor, Task<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TEfCoreDbContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task ExecuteEfDbContextAsync<T>(Func<TEfCoreDbContext, IQueryProcessor, Task<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TEfCoreDbContext>(), sp.GetRequiredService<IQueryProcessor>()));

    public async Task<int> InsertEfDbContextAsync<T>(params T[] entities) where T : class
    {
        return await ExecuteEfDbContextAsync(async db =>
        {
            foreach (var entity in entities.ToList())
            {
                db.Set<T>().Add(entity);
            }

            return await db.SaveChangesAsync();
        });
    }

    public async Task<int> InsertEfDbContextAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);

            return db.SaveChangesAsync();
        });
    }

    public async Task<int> InsertEfDbContextAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return db.SaveChangesAsync();
        });
    }

    public async Task<int> InsertEfDbContextAsync<TEntity, TEntity2, TEntity3>(
        TEntity entity,
        TEntity2 entity2,
        TEntity3 entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return db.SaveChangesAsync();
        });
    }

    public async Task<int> InsertEfDbContextAsync<TEntity, TEntity2, TEntity3, TEntity4>(
        TEntity entity,
        TEntity2 entity2,
        TEntity3 entity3,
        TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return await ExecuteEfDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return db.SaveChangesAsync();
        });
    }

    public Task<T?> FindEfDbContextAsync<T>(object id) where T : class
    {
        return ExecuteEfDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
    }
}
