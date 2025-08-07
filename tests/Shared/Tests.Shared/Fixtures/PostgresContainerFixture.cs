using BuildingBlocks.Core.Extensions;
using Dapper;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Tests.Shared.Helpers;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Tests.Shared.Fixtures;

public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;
    public PostgresContainerOptions PostgresContainerOptions { get; }
    public PostgreSqlContainer PostgresContainer { get; }
    public int HostPort => PostgresContainer.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);
    public string ConnectionString => PostgresContainer.GetConnectionString();
    public int TcpContainerPort => PostgreSqlBuilder.PostgreSqlPort;

    public PostgresContainerFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        PostgresContainerOptions = ConfigurationHelper.BindOptions<PostgresContainerOptions>();
        PostgresContainerOptions.NotBeNull();

        var postgresContainerBuilder = new PostgreSqlBuilder()
            .WithDatabase(PostgresContainerOptions.DatabaseName)
            .WithCleanUp(true)
            .WithName(PostgresContainerOptions.Name)
            .WithImage(PostgresContainerOptions.ImageName);

        PostgresContainer = postgresContainerBuilder.Build();
    }

    public async ValueTask InitializeAsync()
    {
        await PostgresContainer.StartAsync();
        _messageSink.OnMessage(
            new DiagnosticMessage(
                $"Postgres fixture started on Host port {HostPort} and container tcp port {TcpContainerPort}..."
            )
        );
    }

    public async Task ResetDbAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(PostgresContainer.GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            var checkpoint = await Respawner.CreateAsync(
                connection,
                new RespawnerOptions { DbAdapter = DbAdapter.Postgres }
            );

            // https://github.com/jbogard/Respawn/issues/108
            // https://github.com/jbogard/Respawn/pull/115 - fixed
            await checkpoint.ResetAsync(connection)!;
        }
        catch (Exception e)
        {
            _messageSink.OnMessage(new DiagnosticMessage(e.Message));
        }
    }

    public async ValueTask DisposeAsync()
    {
        await PostgresContainer.StopAsync();
        await PostgresContainer.DisposeAsync(); //important for the event to cleanup to be fired!
        _messageSink.OnMessage(new DiagnosticMessage("Postgres fixture stopped."));
    }
}

public sealed class PostgresContainerOptions
{
    public string Name { get; set; } = "postgres_" + Guid.NewGuid();
    public string ImageName { get; set; } = "postgres:latest";
    public string DatabaseName { get; set; } = "test_db";
}
