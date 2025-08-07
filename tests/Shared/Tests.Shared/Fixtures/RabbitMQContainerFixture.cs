using BuildingBlocks.Core.Extensions;
using Testcontainers.RabbitMq;
using Tests.Shared.Helpers;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Tests.Shared.Fixtures;

//https://github.com/EasyNetQ/EasyNetQ/wiki
//https://github.com/thinkco/rabbitmq-httpclient-dotnet
//https://www.planetgeek.ch/2015/08/16/cleaning-up-queues-and-exchanges-on-rabbitmq/
//https://www.planetgeek.ch/2015/08/31/cleanup-code-for-cleaning-up-queues-and-exchanges-on-rabbitmq/

public class RabbitMQContainerFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;
    public RabbitMqContainer Container { get; }
    public int ApiPort => Container.GetMappedPublicPort(15672);
    public int HostPort => Container.GetMappedPublicPort(RabbitMqBuilder.RabbitMqPort);
    public int TcpContainerPort => RabbitMqBuilder.RabbitMqPort;
    public RabbitMQContainerOptions RabbitMqContainerOptions { get; }

    public RabbitMQContainerFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        RabbitMqContainerOptions = ConfigurationHelper.BindOptions<RabbitMQContainerOptions>();
        RabbitMqContainerOptions.NotBeNull();

        var rabbitmqContainerBuilder = new RabbitMqBuilder()
            .WithUsername(RabbitMqContainerOptions.UserName)
            .WithPassword(RabbitMqContainerOptions.Password)
            // set custom host http port for container http port 15672, beside of automatic tcp port will assign for container port 5672 (default port)
            .WithPortBinding(15672, true)
            // we could comment this line, this is default port for testcontainer
            .WithPortBinding(5672, true)
            .WithCleanUp(true)
            .WithName(RabbitMqContainerOptions.Name)
            .WithImage(RabbitMqContainerOptions.ImageName);

        Container = rabbitmqContainerBuilder.Build();
    }

    public async Task CleanupQueuesAsync(CancellationToken cancellationToken = default)
    {
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://{Container.Hostname}:{ApiPort}") };
        // Add Basic Auth for RabbitMQ management
        var byteArray = System.Text.Encoding.ASCII.GetBytes(
            $"{RabbitMqContainerOptions.UserName}:{RabbitMqContainerOptions.Password}"
        );
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(byteArray)
        );

        // 1. Get all queues
        var response = await httpClient.GetAsync("/api/queues", cancellationToken);
        response.EnsureSuccessStatusCode();

        var queuesJson = await response.Content.ReadAsStringAsync(cancellationToken);
        // minimal model for queue info just to get vhost and name
        var queues = System.Text.Json.JsonSerializer.Deserialize<List<RabbitMqQueueInfo>>(queuesJson);

        if (queues is null)
            return;

        // 2. Purge each queue
        foreach (var queue in queues)
        {
            // Must url-encode vhost and queue name
            var vhost = Uri.EscapeDataString(queue.vhost ?? "/");
            var name = Uri.EscapeDataString(queue.name ?? "");
            // POST to purge endpoint
            var purgeResp = await httpClient.DeleteAsync($"/api/queues/{vhost}/{name}/contents", cancellationToken);
            // If the queue doesn't exist or can't be purged, ignore error
        }
    }

    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync();
        _messageSink.OnMessage(
            new DiagnosticMessage(
                $"RabbitMq fixture started on api port {ApiPort}, container tcp port {TcpContainerPort} and host port: {HostPort}..."
            )
        );
    }

    public async ValueTask DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync(); //important for the event to cleanup to be fired!
        _messageSink.OnMessage(new DiagnosticMessage("RabbitMq fixture stopped."));
    }

    private class RabbitMqQueueInfo
    {
        public string? name { get; set; }
        public string? vhost { get; set; }
    }
}

public sealed class RabbitMQContainerOptions
{
    public string Name { get; set; } = "rabbitmq_" + Guid.NewGuid();
    public ushort Port { get; set; } = 5672;
    public string ImageName { get; set; } = "rabbitmq:management";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
