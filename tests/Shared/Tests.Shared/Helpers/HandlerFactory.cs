using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Types.Extensions;
using Hypothesist;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Helpers;

public static class HandlerFactory
{
    public static IMessageHandler<T> AsMessageHandler<T>(this Observer<T> observer)
        where T : class, IMessage
    {
        return new SimpleMessageConsumer<T>(observer);
    }

    public static IConsumer<T> AsConsumer<T>(this Observer<T> observer)
        where T : class, IMessage
    {
        return new MassTransitSimpleMessageConsumer<T>(observer);
    }

    public static IConsumer<TMessage> AsConsumer<TMessage, TConsumer>(
        this Observer<TMessage> observer,
        IServiceProvider serviceProvider
    )
        where TMessage : class, IMessage
        where TConsumer : IConsumer<TMessage>
    {
        return new MassTransitConsumer<TMessage>(observer, serviceProvider, typeof(TConsumer));
    }

    public static BuildingBlocks.Abstractions.Messaging.MessageHandler<T> AsMessageHandlerDelegate<T>(
        this Observer<T> observer
    )
        where T : class, IMessage
    {
        return (context, cancellationToken) => observer.Add(context.Message, cancellationToken);
    }

    public static IMessageHandler<TMessage> AsMessageHandler<TMessage, TMessageHandler>(
        this Observer<TMessage> hypothesis,
        IServiceProvider serviceProvider
    )
        where TMessage : class, IMessage
        where TMessageHandler : IMessageHandler<TMessage>
    {
        return new MessageConsumer<TMessage>(hypothesis, serviceProvider, typeof(TMessageHandler));
    }
}

internal class MessageConsumer<T>(Observer<T> observer, IServiceProvider serviceProvider, Type internalHandler)
    : IMessageHandler<T>
    where T : class, IMessage
{
    public async Task HandleAsync(IEventEnvelope<T> eventEnvelope, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(internalHandler);
        if (handler is null)
        {
            await observer.Add(null!, cancellationToken);

            return;
        }

        await handler.InvokeMethodWithoutResultAsync("HandleAsync", eventEnvelope, cancellationToken);
        await observer.Add(eventEnvelope.Message, cancellationToken);
    }
}

internal class SimpleMessageConsumer<T>(Observer<T> observer) : IMessageHandler<T>
    where T : class, IMessage
{
    public Task HandleAsync(IEventEnvelope<T> eventEnvelope, CancellationToken cancellationToken = default)
    {
        return observer.Add(eventEnvelope.Message, cancellationToken);
    }
}

internal class MassTransitConsumer<T>(Observer<T> hypothesis, IServiceProvider serviceProvider, Type internalHandler)
    : IConsumer<T>
    where T : class, IMessage
{
    public async Task Consume(ConsumeContext<T> context)
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(internalHandler);
        if (handler is null)
        {
            await hypothesis.Add(null!);
            return;
        }

        await handler.InvokeMethodWithoutResultAsync("Consume", context);
        await hypothesis.Add(context.Message);
    }
}

internal class MassTransitSimpleMessageConsumer<TMessage>(Observer<TMessage> observer) : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    public Task Consume(ConsumeContext<TMessage> context)
    {
        return observer.Add(context.Message);
    }
}
