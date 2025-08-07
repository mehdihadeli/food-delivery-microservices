using BuildingBlocks.Core.Extensions;
using StackExchange.Redis;
using Testcontainers.Redis;
using Tests.Shared.Helpers;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Tests.Shared.Fixtures;

public class RedisContainerFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;
    public RedisContainer Container { get; }
    public string ConnectionString => $"{Container.Hostname}:{Container.GetMappedPublicPort(6379)}";
    public RedisContainerOptions RedisContainerOptions { get; }

    public RedisContainerFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        RedisContainerOptions = ConfigurationHelper.BindOptions<RedisContainerOptions>();
        RedisContainerOptions.NotBeNull();

        Container = new RedisBuilder()
            .WithImage(RedisContainerOptions.ImageName)
            .WithName(RedisContainerOptions.Name)
            .WithPortBinding(6379, true)
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync();
        _messageSink.OnMessage(
            new DiagnosticMessage(
                $"Redis fixture started on port {Container.GetMappedPublicPort(6379)} (connection string: {ConnectionString})..."
            )
        );
    }

    public async ValueTask DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
        _messageSink.OnMessage(new DiagnosticMessage("Redis fixture stopped."));
    }
}

public sealed class RedisContainerOptions
{
    public string Name { get; set; } = "redis_" + Guid.NewGuid();
    public string ImageName { get; set; } = "redis/redis-stack:latest";
}
