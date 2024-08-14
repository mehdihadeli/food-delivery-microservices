using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Persistence.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Sdk;

namespace Tests.Shared.Fixtures;

public class SharedFixtureWithEfCoreAndMongo<TEntryPoint, TEfCoreDbContext, TMongoDbContext>
    : SharedFixtureWithEfCore<TEntryPoint, TEfCoreDbContext>
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

    public Task ExecuteMongoDbContextAsync(Func<TMongoDbContext, Task> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>()));

    public Task ExecuteMongoDbContextAsync(Func<TMongoDbContext, ICommandBus, Task> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<ICommandBus>()));

    public Task<T> ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, ICommandBus, Task<T>> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<ICommandBus>()));

    public Task ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, IQueryBus, Task<T>> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<IQueryBus>()));

    public Task<T> ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, Task<T>> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>()));

    public async Task InsertMongoDbContextAsync<T>(params T[] entities)
        where T : class
    {
        await ExecuteMongoDbContextAsync(async db =>
        {
            await db.GetCollection<T>().InsertManyAsync(entities.ToList());
        });
    }

    public SharedFixtureWithEfCoreAndMongo(IMessageSink messageSink)
        : base(messageSink)
    {
        messageSink.OnMessage(new DiagnosticMessage("Constructing SharedFixtureWithEfCoreAndMongo ..."));
    }
}
