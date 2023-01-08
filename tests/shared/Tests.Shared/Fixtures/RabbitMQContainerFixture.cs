using Ardalis.GuardClauses;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyNetQ.Management.Client;
using Tests.Shared.Helpers;

namespace Tests.Shared.Fixtures;

//https://github.com/EasyNetQ/EasyNetQ/wiki
//https://github.com/thinkco/rabbitmq-httpclient-dotnet
//https://www.planetgeek.ch/2015/08/16/cleaning-up-queues-and-exchanges-on-rabbitmq/
//https://www.planetgeek.ch/2015/08/31/cleanup-code-for-cleaning-up-queues-and-exchanges-on-rabbitmq/

public class RabbitMQContainerFixture : IAsyncLifetime
{
    private readonly RabbitMQContainerOptions _rabbitMqContainerOptions;
    public RabbitMqTestcontainer Container { get; }

    public RabbitMQContainerFixture()
    {
        var rabbitmqContainerOptions = ConfigurationHelper.BindOptions<RabbitMQContainerOptions>();
        Guard.Against.Null(rabbitmqContainerOptions);
        _rabbitMqContainerOptions = rabbitmqContainerOptions;

        var rabbitmqContainerBuilder = new TestcontainersBuilder<RabbitMqTestcontainer>()
            .WithMessageBroker(new RabbitMqTestcontainerConfiguration
            {
                Username = rabbitmqContainerOptions.UserName, Password = rabbitmqContainerOptions.Password,
            })
            // set custom host http port for container http port 15672, beside of automatic tcp port will assign for container port 5672 (default port)
            .WithPortBinding(15673, 15672)
            // we could comment this line, this is default port for testcontainer
            .WithPortBinding(5672, true)
            .WithCleanUp(true)
            .WithName(rabbitmqContainerOptions.Name)
            .WithImage(rabbitmqContainerOptions.ImageName);

        Container = rabbitmqContainerBuilder.Build();
    }

    public async Task CleanupQueuesAsync(CancellationToken cancellationToken = default)
    {
        // https://www.rabbitmq.com/dotnet-api-guide.html#exchanges-and-queues
        // https://www.rabbitmq.com/management.html#http-api
        // http://localhost:15672/api/index.html
        // https://rawcdn.githack.com/rabbitmq/rabbitmq-server/v3.11.4/deps/rabbitmq_management/priv/www/api/index.html
        // https://www.planetgeek.ch/2015/08/16/cleaning-up-queues-and-exchanges-on-rabbitmq/
        // https://www.planetgeek.ch/2015/08/31/cleanup-code-for-cleaning-up-queues-and-exchanges-on-rabbitmq/

        var apiPort = Container.GetMappedPublicPort(15672);

        // here I used rabbitmq http apis (Management Plugin) but also we can also use RabbitMQ client library and channel.ExchangeDelete(), channel.QueueDelete(), official client
        // is not complete for administrative works for example it doesn't have GetAllQueues, GetAllExchanges
        var managementClient = new ManagementClient(
            $"http://{Container.Hostname}",
            Container.Username,
            Container.Password,
            apiPort);

        //Creating new exchange after each publish doesn't support by masstransit and it just creates exchanges in init phase but works for queues
        var queues = await managementClient.GetQueuesAsync(cancellationToken);
        foreach (var queue in queues)
        {
            await managementClient.PurgeAsync(queue, cancellationToken);
        }
    }

    // we can use this method also for when we don't use docker testcontainer
    public async Task DropElementsAsync(CancellationToken cancellationToken = default)
    {
        var apiPort = Container.GetMappedPublicPort(15672);

        // here I used rabbitmq http apis (Management Plugin) but also we can also use RabbitMQ client library and channel.ExchangeDelete(), channel.QueueDelete(), official client
        // is not complete for administrative works for example it doesn't have GetAllQueues, GetAllExchanges
        var managementClient = new ManagementClient(
            $"http://{Container.Hostname}",
            Container.Username,
            Container.Password,
            apiPort);

        var bd = await managementClient.GetBindingsAsync(cancellationToken);
        var bindings = bd.Where(x => !string.IsNullOrEmpty(x.Source) && !string.IsNullOrEmpty(x.Destination));

        foreach (var binding in bindings)
        {
            await managementClient.DeleteBindingAsync(binding, cancellationToken);
        }

        var queues = await managementClient.GetQueuesAsync(cancellationToken);
        foreach (var queue in queues)
        {
            await managementClient.DeleteQueueAsync(queue, cancellationToken);
        }

        //Creating new exchange after each publish doesn't support by masstransit and it just creates exchanges in init phase but works for queues
        var ex = await managementClient.GetExchangesAsync(cancellationToken);
        // ignore default rabbitmq exchanges for deleting
        var exchanges = ex.Where(x => !x.Name.StartsWith("amq.") && !string.IsNullOrWhiteSpace(x.Name));

        foreach (var exchange in exchanges)
        {
            await managementClient.DeleteExchangeAsync(exchange, cancellationToken);
        }
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

    private sealed class RabbitMQContainerOptions
    {
        public string Name { get; set; } = "rabbitmq_" + Guid.NewGuid();
        public ushort Port { get; set; } = 5672;
        public string ImageName { get; set; } = "rabbitmq:management";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}
