using MassTransit;
using MassTransit.Configuration;
using MassTransit.RabbitMqTransport.Configuration;
using RabbitMQ.Client;

namespace BuildingBlocks.Integration.MassTransit;

public class CustomPublishTopologyConvention : IPublishTopologyConvention
{
    public bool TryGetMessagePublishTopologyConvention<T>(out IMessagePublishTopologyConvention<T> convention)
        where T : class
    {
        convention = new CustomMessagePublishTopologyConvention<T>();
        return true;
    }
}

public class CustomMessagePublishTopologyConvention<T> : IMessagePublishTopologyConvention<T>
    where T : class
{
    public bool TryGetMessagePublishTopology(out IMessagePublishTopology<T> messagePublishTopology)
    {
        // Return a custom message publish topology
        messagePublishTopology = new CustomMessagePublishTopology<T>();
        return true;
    }

    // This method is responsible for retrieving a publish topology convention for a given message type T1
    public bool TryGetMessagePublishTopologyConvention<T1>(out IMessagePublishTopologyConvention<T1> convention)
        where T1 : class
    {
        // If T1 is the same as T, return this convention for T1
        if (typeof(T1) == typeof(T))
        {
            convention = (IMessagePublishTopologyConvention<T1>)this;
            return true;
        }

        // If it's a different type, we cannot provide a topology convention
        convention = null;
        return false;
    }
}

// This class represents the custom publish topology for a specific message type T
public class CustomMessagePublishTopology<T>(bool exclude = false) : IMessagePublishTopology<T>
    where T : class
{
    // A flag indicating whether this topology should be excluded
    public bool Exclude { get; private set; } = exclude;

    // Apply custom publish topology using the provided builder
    public void Apply(ITopologyPipeBuilder<PublishContext<T>> builder)
    {
        // Here we apply the custom settings for the exchange
        builder.AddFilter(new CustomPublishTopologyFilter<T>());
    }

    // Attempt to get the publish address by adding the exchange name to the base address
    public bool TryGetPublishAddress(Uri baseAddress, out Uri? publishAddress)
    {
        // The baseAddress is typically the base RabbitMQ URL (e.g., rabbitmq://localhost/)
        // We want to append the exchange name to the base address for the specific message type

        // For example, you can derive the exchange name based on the message type T
        var exchangeName = typeof(T).Name.ToLowerInvariant(); // Example: Exchange name based on message type

        // Build the final publish address (e.g., rabbitmq://localhost/exchange/my-message-type)
        if (baseAddress != null)
        {
            var builder = new UriBuilder(baseAddress) { Path = $"exchange/{exchangeName}" };

            publishAddress = builder.Uri;
            return true;
        }

        publishAddress = null;
        return false;
    }
}

public class CustomPublishTopologyFilter<T> : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        // This is where you configure the exchange for the message
        var exchange = context.GetPayload<RabbitMqExchangeConfigurator>();

        // Set the exchange to be durable and of type 'direct'
        exchange.Durable = true;
        exchange.ExchangeType = ExchangeType.Direct;

        // Continue processing the next filters in the pipeline
        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("CustomPublishTopologyFilter");
    }
}
