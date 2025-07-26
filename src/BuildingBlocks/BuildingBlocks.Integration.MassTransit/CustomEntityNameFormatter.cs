using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Constants;
using BuildingBlocks.Core.Messages;
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
        // Check if T implements IMessageEnvelopeBase
        if (typeof(IMessageEnvelopeBase).IsAssignableFrom(typeof(T)))
        {
            var messageProperty = typeof(T).GetProperty(nameof(IMessageEnvelopeBase.Message));
            if (typeof(IMessage).IsAssignableFrom(messageProperty!.PropertyType))
            {
                return $"{messageProperty.PropertyType.Name.Underscore()}{MessagingConstants.PrimaryExchangePostfix}";
            }
        }

        // Return a default value if T does not implement IEventEnvelop or Message property is not found
        return $"{typeof(T).Name.Underscore()}{MessagingConstants.PrimaryExchangePostfix}";
    }
}

public class CustomEntityNameFormatter<TMessage> : IMessageEntityNameFormatter<TMessage>
    where TMessage : class
{
    public string FormatEntityName()
    {
        // Check if T implements IMessageEnvelopeBase
        if (typeof(IMessageEnvelopeBase).IsAssignableFrom(typeof(TMessage)))
        {
            var messageProperty = typeof(TMessage).GetProperty(nameof(IMessageEnvelopeBase.Message));
            if (typeof(IMessage).IsAssignableFrom(messageProperty!.PropertyType))
            {
                return $"{messageProperty.PropertyType.Name.Underscore()}{MessagingConstants.PrimaryExchangePostfix}";
            }
        }

        // Return a default value if T does not implement IEventEnvelop or Message property is not found
        return $"{typeof(TMessage).Name.Underscore()}{MessagingConstants.PrimaryExchangePostfix}";
    }
}
