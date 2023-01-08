using Ardalis.GuardClauses;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tests.Shared.Helpers;

namespace Tests.Shared.Fixtures;

// we can replace fixture dbcontext with our actual dbcontext in the testfactory
public class EfDbContextTransactionFixture<TContext> : IAsyncLifetime
    where TContext : DbContext
{
    private readonly string? _migrationAssembly;
    private bool _isInnerTransaction;
    private IDbContextTransaction _transaction = null!;
    public TContext DbContext { get; private set; } = default!;
    public PostgreSqlTestcontainer Container { get; }

    public EfDbContextTransactionFixture()
    {
        _migrationAssembly = typeof(TContext).Assembly.GetName().Name;
        var postgresOptions = ConfigurationHelper.BindOptions<PostgresContainerOptions>();
        Guard.Against.Null(postgresOptions);

        var postgresContainerBuilder = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = postgresOptions.DatabaseName,
                Username = postgresOptions.UserName,
                Password = postgresOptions.Password,
            })
            .WithCleanUp(true)
            .WithName(postgresOptions.Name)
            .WithImage(postgresOptions.ImageName);

        Container = postgresContainerBuilder.Build();
    }

    public async Task ResetAsync()
    {
        try
        {
            if (_isInnerTransaction == false)
            {
                await _transaction.RollbackAsync();
                await ConfigTransaction();
            }
        }
        catch
        {
            if (_isInnerTransaction == false)
                await _transaction.RollbackAsync();

            throw;
        }
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
        var options = ConfigurationHelper.BindOptions<PostgresOptions>();
        options.ConnectionString = Container.ConnectionString;
        options.MigrationAssembly = _migrationAssembly;

        var optionBuilder = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(
                options.ConnectionString,
                optionsBuilder =>
                    optionsBuilder.MigrationsAssembly(options.MigrationAssembly))
            .UseSnakeCaseNamingConvention()
            .Options;

        DbContext = typeof(TContext).CreateInstanceFromType<TContext>(optionBuilder);
        await DbContext.Database.MigrateAsync();
        await ConfigTransaction();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await Container.StopAsync();
        await Container.DisposeAsync(); //important for the event to cleanup to be fired!
    }

    private async Task ConfigTransaction()
    {
        var strategy = DbContext.Database.CreateExecutionStrategy();
        _isInnerTransaction = DbContext.Database.CurrentTransaction is not null;

        await strategy.ExecuteAsync(async () =>
        {
            // https://www.thinktecture.com/en/entity-framework-core/use-transactionscope-with-caution-in-2-1/
            // https://github.com/dotnet/efcore/issues/6233#issuecomment-242693262
            _transaction = await DbContext.Database.BeginTransactionAsync();
        });
    }

    private sealed class PostgresContainerOptions
    {
        public string Name { get; set; } = "postgres_" + Guid.NewGuid();
        public string ImageName { get; set; } = "postgres:latest";
        public string DatabaseName { get; set; } = "test_db";
        public string UserName { get; set; } = "postgres";
        public string Password { get; set; } = "postgres";
    }
}
