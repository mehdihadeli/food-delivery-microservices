using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.Mongo;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BuildingBlocks.Persistence.Mongo;

// https://www.thecodebuzz.com/mongodb-repository-implementation-unit-testing-net-core-example/
public class MongoDbContext(IMongoClient mongoClient, IMongoDatabase mongoDatabase)
    : IMongoDbContext,
        ITxDbContextExecution
{
    public IClientSessionHandle? Session { get; set; }
    public IMongoDatabase Database { get; } = mongoDatabase;
    public IMongoClient MongoClient { get; } = mongoClient;
    protected readonly IList<Func<Task>> Commands = new List<Func<Task>>();

    // Every command will be stored, and it'll be processed at SaveChanges

    public IMongoCollection<T> GetCollection<T>(string? name = null)
    {
        return Database.GetCollection<T>(name ?? typeof(T).Name.Pluralize().Underscore());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = Commands.Count;

        // Standalone servers do not support transactions.
        using (Session = await MongoClient.StartSessionAsync(cancellationToken: cancellationToken))
        {
            try
            {
                Session.StartTransaction();

                await SaveAction();

                await Session.CommitTransactionAsync(cancellationToken);
            }
            catch (NotSupportedException notSupportedException)
            {
                await SaveAction();
            }
            catch (Exception ex)
            {
                await Session.AbortTransactionAsync(cancellationToken);
                Commands.Clear();
                throw;
            }
        }

        Commands.Clear();

        return result;
    }

    private async Task SaveAction()
    {
        var commandTasks = Commands.Select(c => c());

        await Task.WhenAll(commandTasks);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        Session = await MongoClient.StartSessionAsync(cancellationToken: cancellationToken);
        Session.StartTransaction();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Session is { IsInTransaction: true })
            await Session.CommitTransactionAsync(cancellationToken);

        Session?.Dispose();
    }

    public async Task RollbackTransaction(CancellationToken cancellationToken = default)
    {
        await Session?.AbortTransactionAsync(cancellationToken)!;
    }

    public void AddCommand(Func<Task> func)
    {
        Commands.Add(func);
    }

    public async Task ExecuteTransactionalAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync(cancellationToken);
        try
        {
            await action();

            await CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransaction(cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteTransactionalAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default
    )
    {
        await BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action();

            await CommitTransactionAsync(cancellationToken);

            return result;
        }
        catch
        {
            await RollbackTransaction(cancellationToken);
            throw;
        }
    }
}
