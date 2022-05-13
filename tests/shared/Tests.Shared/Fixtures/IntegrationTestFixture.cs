using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Persistence.EfCore.Postgres;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Respawn;
using Respawn.Graph;
using Xunit.Abstractions;

namespace Tests.Shared.Fixtures;

// Ref: https://github.com/jbogard/ContosoUniversityDotNetCore-Pages/blob/master/ContosoUniversity.IntegrationTests/SliceFixture.cs
public class IntegrationTestFixture<TEntryPoint, TDbContext> : IntegrationTestFixture<TEntryPoint>
    where TDbContext : DbContext
    where TEntryPoint : class
{
    public async Task ExecuteDbContextAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                await action(scope.ServiceProvider);

                await dbContext.Database.CommitTransactionAsync();
            }
            catch(Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task<T> ExecuteDbContextAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = ServiceProvider.CreateScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider);

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

    public Task ExecuteDbContextAsync(Func<TDbContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TDbContext>()));

    public Task ExecuteDbContextAsync(Func<TDbContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TDbContext>()).AsTask());

    public Task ExecuteDbContextAsync(Func<TDbContext, ICommandProcessor, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TDbContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<TDbContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TDbContext>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<TDbContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TDbContext>()).AsTask());

    public Task<T> ExecuteDbContextAsync<T>(Func<TDbContext, ICommandProcessor, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TDbContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task InsertAsync<T>(params T[] entities) where T : class
    {
        return ExecuteDbContextAsync(db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2,
        TEntity3 entity3, TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return db.SaveChangesAsync();
        });
    }

    public Task<T?> FindAsync<T>(object id) where T : class
    {
        return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
    }

    public Task PublishMessageAsync<TMessage>(TMessage message, IDictionary<string, object?>? headers = null) where
        TMessage : class, IMessage
    {
        return ExecuteScopeAsync(sp =>
        {
            var bus = sp.GetRequiredService<IBus>();

            return bus.PublishAsync(message, headers, CancellationToken.None);
        });
    }
}

public class IntegrationTestFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private readonly CustomWebApplicationFactory<TEntryPoint> _factory;
    private readonly Checkpoint _checkpoint;

    public IntegrationTestFixture()
    {
        // Ref: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#basic-tests-with-the-default-webapplicationfactory
        _factory = new CustomWebApplicationFactory<TEntryPoint>();
        _checkpoint = new Checkpoint {TablesToIgnore = new List<Table> {new("__EFMigrationsHistory")}.ToArray()};
    }

    public IServiceProvider ServiceProvider => _factory.Services;
    public IConfiguration Configuration => _factory.Configuration;
    public ILogger<IntegrationTestFixture<TEntryPoint>> Logger =>
        ServiceProvider.GetRequiredService<ILogger<IntegrationTestFixture<TEntryPoint>>>();
    public IBus Bus =>
        ServiceProvider.GetRequiredService<IBus>();

    private async Task ResetState()
    {
        try
        {
            var postgresOptions =
                OptionsHelper.GetOptions<BuildingBlocks.Persistence.EfCore.Postgres.PostgresOptions>(
                    nameof(PostgresOptions));
            if (!string.IsNullOrEmpty(postgresOptions.ConnectionString))
                await _checkpoint.Reset(postgresOptions.ConnectionString);
        }
        catch
        {
            // ignored
        }
    }

    public void SetOutputHelper(ITestOutputHelper outputHelper)
    {
        _factory.OutputHelper = outputHelper;
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        loggerFactory.AddXUnit(outputHelper);
    }

    public IHttpContextAccessor HttpContextAccessor =>
        ServiceProvider.GetRequiredService<IHttpContextAccessor>();

    public HttpClient CreateNewClient(Action<IServiceCollection>? services = null) =>
        _factory.WithWebHostBuilder(b =>
                b.ConfigureTestServices(sv =>
                {
                    services?.Invoke(sv);
                }))
            .CreateClient();

    public void RegisterTestServices(Action<IServiceCollection> services) =>
        _factory.TestRegistrationServices = services;

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();

        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = ServiceProvider.CreateScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    public virtual async Task InitializeAsync()
    {
        await ResetState();
    }

    public virtual Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }
}
