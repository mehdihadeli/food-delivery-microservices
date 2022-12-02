using Ardalis.GuardClauses;
using Dapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Npgsql;
using Respawn;
using Tests.Shared.Helpers;

namespace Tests.Shared.Fixtures;

public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgresContainerOptions _postgresContainerOptions;
    public PostgreSqlTestcontainer Container { get; }

    public PostgresContainerFixture()
    {
        var postgresOptions = ConfigurationHelper.GetOptions<PostgresContainerOptions>();
        Guard.Against.Null(postgresOptions);
        _postgresContainerOptions = postgresOptions;

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

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task ResetDbAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(Container.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await CheckForExistingDatabase(connection);

            var checkpoint =
                await Respawner.CreateAsync(connection, new RespawnerOptions {DbAdapter = DbAdapter.Postgres});
            await checkpoint.ResetAsync(connection)!;
        }
        catch (Exception e)
        {
            throw new Exception("Error in ResetPostgresState of ReSpawner.");
        }
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync(); //important for the event to cleanup to be fired!
    }

    private async Task CheckForExistingDatabase(NpgsqlConnection connection)
    {
        var existsDb = await connection.ExecuteScalarAsync<bool>(
            "SELECT 1 FROM  pg_catalog.pg_database WHERE datname= @dbname", param: new {dbname = Container.Database});
        if (existsDb == false)
        {
            await connection.ExecuteAsync(
                "CREATE DATABASE @DBName",
                param: new {DBName = _postgresContainerOptions.DatabaseName});
        }

        // //https://github.com/jbogard/Respawn/issues/108
        // var existsFoo = await connection.ExecuteScalarAsync<bool>(
        //     "SELECT EXISTS (SELECT FROM information_schema.tables WHERE  table_schema = 'foo' AND table_name = 'public')"
        // );
        // if (existsFoo == false)
        // {
        //     await connection.ExecuteAsync(
        //         "create table \"foo\" (value int)");
        // }
    }
}

public class PostgresContainerOptions
{
    public string Name { get; set; } = "postgres_" + Guid.NewGuid();
    public string ImageName { get; set; } = "postgres:latest";
    public string DatabaseName { get; set; } = "test_db";
    public string UserName { get; set; } = "postgres";
    public string Password { get; set; } = "postgres";
}
