using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Messaging;
using Humanizer;
using MassTransit;

namespace BuildingBlocks.Integration.MassTransit;

/// <summary>
/// Setting primary exchange name for each entity type globally.
/// </summary>
public class CustomEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        // Check if T implements IEventEnvelope
        if (typeof(IEventEnvelope).IsAssignableFrom(typeof(T)))
        {
            var messageProperty = typeof(T).GetProperty(nameof(IEventEnvelope.Message));
            if (typeof(IMessage).IsAssignableFrom(messageProperty!.PropertyType))
            {
                return $"{messageProperty.PropertyType.Name.Underscore()}{MessagingConstants.PrimaryExchangePostfix}";
            }
        }

        // Return a default value if T does not implement IEventEnvelop or Message property is not found
        return $"{typeof(T).Name.Underscore()}{MessagingConstants.PrimaryExchangePostfix}";
    }
}
