using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Wolverine.RabbitMQ;

namespace BuildingBlocks.Integration.Wolverine;

internal sealed class RabbitMqTopologyProvisioningHostedService(
    string connectionString,
    ILogger<RabbitMqTopologyProvisioningHostedService> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var topologies = WolverineMessageTopologyExtensions.GetRegisteredListenerTopologies();
        var publishTopologies = WolverineMessageTopologyExtensions.GetRegisteredPublishTopologies();
        logger.LogInformation(
            "Provisioning RabbitMQ topology. Queue count: {QueueCount}. Publish exchange count: {PublishExchangeCount}. Queues: {Queues}. Publish exchanges: {PublishExchanges}",
            topologies.Count,
            publishTopologies.Count,
            string.Join(", ", topologies.Select(x => $"{x.QueueName}:{x.ExchangeName}:{x.RoutingKey}")),
            string.Join(", ", publishTopologies.Select(x => x.ExchangeName))
        );

        if (topologies.Count == 0 && publishTopologies.Count == 0)
        {
            logger.LogWarning("No RabbitMQ topologies were registered before provisioning startup.");
            return;
        }

        try
        {
            var connectionFactory = new ConnectionFactory { Uri = new Uri(connectionString) };

            await using var connection = await connectionFactory.CreateConnectionAsync(
                cancellationToken: cancellationToken
            );
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            foreach (var topology in publishTopologies)
            {
                logger.LogInformation("Declaring RabbitMQ publish exchange {ExchangeName}", topology.ExchangeName);

                await channel.ExchangeDeclareAsync(
                    topology.ExchangeName,
                    RabbitMQ.Client.ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken
                );
            }

            foreach (var topology in topologies)
            {
                logger.LogInformation(
                    "Declaring RabbitMQ topology for queue {QueueName}, exchange {ExchangeName}, routing key {RoutingKey}",
                    topology.QueueName,
                    topology.ExchangeName,
                    topology.RoutingKey
                );

                await channel.ExchangeDeclareAsync(
                    topology.ExchangeName,
                    RabbitMQ.Client.ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken
                );

                await channel.QueueDeclareAsync(
                    topology.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken
                );

                await channel.QueueBindAsync(
                    topology.QueueName,
                    topology.ExchangeName,
                    topology.RoutingKey,
                    arguments: null,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RabbitMQ topology provisioning failed before Wolverine listener startup.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
