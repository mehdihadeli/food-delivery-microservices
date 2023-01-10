using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Persistence.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Fixtures;

public class
    SharedFixtureWithEfCoreAndMongo<TEntryPoint, TEfCoreDbContext, TMongoDbContext> : SharedFixtureWithEfCore<
        TEntryPoint, TEfCoreDbContext>
    where TEfCoreDbContext : DbContext
    where TMongoDbContext : MongoDbContext
    where TEntryPoint : class
{
    public async Task ExecuteMongoDbContextAsync(Func<IServiceProvider, TMongoDbContext, Task> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        await ExecuteScopeAsync(sp => action(scope.ServiceProvider, sp.GetRequiredService<TMongoDbContext>()));
    }

    public async Task<T> ExecuteMongoDbContextAsync<T>(Func<IServiceProvider, TMongoDbContext, Task<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        return await ExecuteScopeAsync(sp => action(scope.ServiceProvider, sp.GetRequiredService<TMongoDbContext>()));
    }

    public Task ExecuteMongoDbContextAsync(Func<TMongoDbContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>()));

    public Task ExecuteMongoDbContextAsync(Func<TMongoDbContext, ICommandProcessor, Task> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task<T> ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, ICommandProcessor, Task<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, IQueryProcessor, Task<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<IQueryProcessor>()));

    public Task<T> ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>()));

    public async Task InsertMongoDbContextAsync<T>(string collectionName, params T[] entities) where T : class
    {
        await ExecuteMongoDbContextAsync(async db =>
        {
            await db.GetCollection<T>(collectionName).InsertManyAsync(entities.ToList());
        });
    }
}
