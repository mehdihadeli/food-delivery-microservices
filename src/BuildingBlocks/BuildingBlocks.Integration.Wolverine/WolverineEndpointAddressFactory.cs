using Wolverine.RabbitMQ;

namespace BuildingBlocks.Integration.Wolverine;

internal static class WolverineEndpointAddressFactory
{
    internal static Uri Get(string exchangeOrTopic, string? queue, string routingKey, bool configureConsumeTopology)
    {
        if (!configureConsumeTopology || !string.IsNullOrWhiteSpace(queue))
        {
            return RabbitMqEndpointUri.Routing(exchangeOrTopic, queue ?? routingKey);
        }

        return RabbitMqEndpointUri.Exchange(exchangeOrTopic);
    }
}
