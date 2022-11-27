using DotNet.Testcontainers.Containers;
using Tests.Shared.TestContainers;

namespace Tests.Shared.Fixtures;

public class ContainersFixture : IAsyncLifetime
{
    public ContainersFixture()
    {
        PostgresTestContainer = new PostgresTestContainer(new PostgresContainerOptions()).Container;
    }

    public  PostgreSqlTestcontainer PostgresTestContainer { get;}

    public Task InitializeAsync()
    {
        return PostgresTestContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return PostgresTestContainer.StopAsync();
    }
}
