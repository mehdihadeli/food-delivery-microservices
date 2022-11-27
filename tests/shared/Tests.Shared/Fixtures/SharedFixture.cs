using AutoBogus;
using WireMock.Server;

namespace Tests.Shared.Fixtures;

public class SharedFixture : IAsyncLifetime
{
    public ContainersFixture ContainersFixture { get; }
    public WireMockServer WireMockServer { get; private set; } = default!;

    public SharedFixture()
    {
        ContainersFixture = new ContainersFixture();

        AutoFaker.Configure(
            b =>
            {
                // configure global AutoBogus settings here
                b.WithRecursiveDepth(3)
                    .WithTreeDepth(1)
                    .WithRepeatCount(1);
            });
    }

    public async Task InitializeAsync()
    {
        await ContainersFixture.InitializeAsync();

        // new WireMockServer() is equivalent to call WireMockServer.Start()
        WireMockServer = WireMockServer.Start();
    }

    public async Task DisposeAsync()
    {
        await ContainersFixture.DisposeAsync();
        WireMockServer.Stop();
    }
}
