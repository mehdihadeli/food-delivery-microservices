namespace BuildingBlocks.Core.Events.Extensions;

using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

public static class EventsExtensions
{
    public static IEnumerable<Type> GetHandledMessageTypes(this Assembly[] assemblies)
    {
        var messageHandlerTypes = typeof(IMessageHandler<>)
            .GetAllTypesImplementingOpenGenericInterface(assemblies)
            .ToList();

        var inheritsTypes = messageHandlerTypes
            .SelectMany(x => x.GetInterfaces())
            .Where(x =>
                x.GetInterfaces().Any(i => i.IsGenericType) && x.GetGenericTypeDefinition() == typeof(IMessageHandler<>)
            );

        foreach (var inheritsType in inheritsTypes)
        {
            var messageType = inheritsType.GetGenericArguments().First();
            if (messageType.IsAssignableTo(typeof(IMessage)))
            {
                yield return messageType;
            }
        }
    }

    public static IEnumerable<Type> GetHandledMessageEnvelopTypes(this Assembly[] assemblies)
    {
        var messageHandlerTypes = typeof(IMessageEnvelopeHandler<>)
            .GetAllTypesImplementingOpenGenericInterface(assemblies)
            .ToList();

        var inheritsTypes = messageHandlerTypes
            .SelectMany(x => x.GetInterfaces())
            .Where(x =>
                x.GetInterfaces().Any(i => i.IsGenericType)
                && x.GetGenericTypeDefinition() == typeof(IMessageEnvelopeHandler<>)
            );

        foreach (var inheritsType in inheritsTypes)
        {
            var messageType = inheritsType.GetGenericArguments().First();
            if (messageType.IsAssignableTo(typeof(IMessageEnvelopeBase)))
            {
                yield return messageType;
            }
        }
    }

    public static IEnumerable<Type> GetHandledDomainNotificationEventTypes(this Assembly[] assemblies)
    {
        var messageHandlerTypes = typeof(IDomainNotificationEventHandler<,>)
            .GetAllTypesImplementingOpenGenericInterface(assemblies)
            .ToList();

        var inheritsTypes = messageHandlerTypes
            .SelectMany(x => x.GetInterfaces())
            .Where(x =>
                x.GetInterfaces().Any(i => i.IsGenericType)
                && x.GetGenericTypeDefinition() == typeof(IDomainNotificationEventHandler<,>)
            );

        foreach (var inheritsType in inheritsTypes)
        {
            var messageType = inheritsType.GetGenericArguments().First();
            if (messageType.IsAssignableTo(typeof(IDomainNotificationEvent<IDomainEvent>)))
            {
                yield return messageType;
            }
        }
    }

    public static IEnumerable<Type> GetHandledDomainEventTypes(this Assembly[] assemblies)
    {
        var messageHandlerTypes = typeof(IDomainEventHandler<>)
            .GetAllTypesImplementingOpenGenericInterface(assemblies)
            .ToList();

        var inheritsTypes = messageHandlerTypes
            .SelectMany(x => x.GetInterfaces())
            .Where(x =>
                x.GetInterfaces().Any(i => i.IsGenericType)
                && x.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
            );

        foreach (var inheritsType in inheritsTypes)
        {
            var messageType = inheritsType.GetGenericArguments().First();
            if (messageType.IsAssignableTo(typeof(IDomainEvent)))
            {
                yield return messageType;
            }
        }
    }

    public static IDomainNotificationEvent<IDomainEvent>? GetWrappedDomainNotificationEvent(
        this IDomainEvent domainEvent
    )
    {
        domainEvent.NotBeNull();

        // run only for domain-events that have a `IHaveNotificationEvent` marker to discover as WrappedDomainNotificationEvent
        if (typeof(IHaveNotificationEvent).IsAssignableFrom(domainEvent.GetType()))
        {
            Type genericType = typeof(DomainNotificationEventWrapper<>).MakeGenericType(domainEvent.GetType());

            IDomainNotificationEvent<IDomainEvent>? domainNotificationEvent = (IDomainNotificationEvent<IDomainEvent>?)
                Activator.CreateInstance(genericType, domainEvent);

            if (domainNotificationEvent is not null)
            {
                return domainNotificationEvent;
            }
        }

        return null;
    }

    public static IIntegrationEvent? GetWrappedIntegrationEvent(this IDomainEvent domainEvent)
    {
        domainEvent.NotBeNull();

        // run only for domain-events that have a `IHaveExternalEvent` marker to discover as WrappedIntegrationEvents
        if (typeof(IHaveExternalEvent).IsAssignableFrom(domainEvent.GetType()))
        {
            Type genericType = typeof(IntegrationEventWrapper<>).MakeGenericType(domainEvent.GetType());

            IIntegrationEvent? domainNotificationEvent = (IIntegrationEvent?)
                Activator.CreateInstance(genericType, domainEvent);

            if (domainNotificationEvent is not null)
            {
                return domainNotificationEvent;
            }
        }

        return null;
    }
}
