using Ardalis.GuardClauses;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Reflection.Extensions;
using BuildingBlocks.Core.Types.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Testcontainers.PostgreSql;
using Tests.Shared.Helpers;
using Xunit.Sdk;

namespace Tests.Shared.Fixtures;

// we can replace fixture dbcontext with our actual dbcontext in the testfactory
public class EfDbContextTransactionFixture<TContext> : IAsyncLifetime
    where TContext : DbContext
{
    private readonly IMessageSink _messageSink;
    private readonly string? _migrationAssembly;
    private bool _isInnerTransaction;
    private IDbContextTransaction _transaction = null!;
    public PostgresContainerOptions PostgresContainerOptions { get; }
    public TContext DbContext { get; private set; } = default!;
    public PostgreSqlContainer Container { get; }
    public int HostPort => Container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);
    public int TcpContainerPort => PostgreSqlBuilder.PostgreSqlPort;

    public EfDbContextTransactionFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        _migrationAssembly = typeof(TContext).Assembly.GetName().Name;
        PostgresContainerOptions = ConfigurationHelper.BindOptions<PostgresContainerOptions>();
        Guard.Against.Null(PostgresContainerOptions);

        var postgresContainerBuilder = new PostgreSqlBuilder()
            .WithDatabase(PostgresContainerOptions.DatabaseName)
            .WithUsername(PostgresContainerOptions.UserName)
            .WithPassword(PostgresContainerOptions.Password)
            .WithCleanUp(true)
            .WithName(PostgresContainerOptions.Name)
            .WithImage(PostgresContainerOptions.ImageName);

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
        options.ConnectionString = Container.GetConnectionString();
        options.MigrationAssembly = _migrationAssembly;

        var optionBuilder = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(
                options.ConnectionString,
                optionsBuilder => optionsBuilder.MigrationsAssembly(options.MigrationAssembly)
            )
            .UseSnakeCaseNamingConvention()
            .Options;

        DbContext = typeof(TContext).CreateInstanceFromType<TContext>(optionBuilder);
        await DbContext.Database.MigrateAsync();
        await ConfigTransaction();
        _messageSink.OnMessage(new DiagnosticMessage($"EfDbContextTransactionFixture fixture started..."));
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await Container.StopAsync();
        await Container.DisposeAsync(); //important for the event to cleanup to be fired!
        _messageSink.OnMessage(new DiagnosticMessage($"EfDbContextTransactionFixture fixture stopped"));
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
}
