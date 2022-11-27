using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Tests.Shared.TestContainers;

public class PostgresTestContainer : IAsyncLifetime
{
    public PostgreSqlTestcontainer Container { get; }

    public PostgresTestContainer(PostgresContainerOptions options)
    {
        var postgresContainerBuilder = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = options.DatabaseName, Username = options.UserName, Password = options.Password,
            })
            .WithImage(options.ImageName);

        Container = postgresContainerBuilder.Build();
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync(); //important for the event to cleanup to be fired!
    }
}

public class PostgresContainerOptions
{
    public string Name { get; set; } = "postgres_" + Guid.NewGuid();
    public string ImageName { get; set; } = "postgres";
    public string DatabaseName { get; set; } = "testdb";
    public string UserName { get; set; } = "postgres";
    public string Password { get; set; } = "postgres";
}
