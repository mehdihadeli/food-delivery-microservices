using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Types.Extensions;
using Hypothesist;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Helpers;

public static class HandlerFactory
{
    public static IMessageHandler<T> AsMessageHandler<T>(this Observer<T> hypothesis)
        where T : class, IMessage
    {
        return new SimpleMessageConsumer<T>(hypothesis);
    }

    public static IConsumer<T> AsConsumer<T>(this Observer<T> hypothesis)
        where T : class, IMessage
    {
        return new MassTransitSimpleMessageConsumer<T>(hypothesis);
    }

    public static IConsumer<TMessage> AsConsumer<TMessage, TConsumer>(
        this Observer<TMessage> hypothesis,
        IServiceProvider serviceProvider
    )
        where TMessage : class, IMessage
        where TConsumer : IConsumer<TMessage>
    {
        return new MassTransitConsumer<TMessage>(hypothesis, serviceProvider, typeof(TConsumer));
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

    public static IMessageHandler<TMessage> AsMessageEnvelopeHandler<TMessage, TMessageHandler>(
        this Observer<TMessage> observer,
        IServiceProvider serviceProvider
    )
        where TMessage : class, IMessage
        where TMessageHandler : IMessageEnvelopeHandler<TMessage>
    {
        return new MessageConsumer<TMessage>(observer, serviceProvider, typeof(TMessageHandler));
    }
}

internal class MessageConsumer<T>(Observer<T> observer, IServiceProvider serviceProvider, Type internalHandler)
    : IMessageHandler<T>
    where T : class, IMessage
{
    public async Task Handle(T message, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(internalHandler);
        if (handler is null)
        {
            await observer.Add(null!, cancellationToken);
            return;
        }

        await handler.InvokeMethodWithoutResultAsync("HandleAsync", message, cancellationToken);
        await observer.Add(message, cancellationToken);
    }
}

internal class SimpleMessageConsumer<T>(Observer<T> observer) : IMessageHandler<T>
    where T : class, IMessage
{
    public async Task Handle(T message, CancellationToken cancellationToken = default)
    {
        await observer.Add(message, cancellationToken);
    }
}

internal class MessageEnvelopeConsumer<T>(Observer<T> observer, IServiceProvider serviceProvider, Type internalHandler)
    : IMessageEnvelopeHandler<T>
    where T : class, IMessage
{
    public async Task Handle(IMessageEnvelope<T> messageEnvelope, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(internalHandler);
        if (handler is null)
        {
            await observer.Add(null!, cancellationToken);
            return;
        }

        await handler.InvokeMethodWithoutResultAsync("HandleAsync", messageEnvelope, cancellationToken);
        await observer.Add(messageEnvelope.Message, cancellationToken);
    }
}

internal class SimpleMessageEnvelopeConsumer<T>(Observer<T> observer) : IMessageEnvelopeHandler<T>
    where T : class, IMessage
{
    public async Task Handle(IMessageEnvelope<T> messageEnvelope, CancellationToken cancellationToken = default)
    {
        await observer.Add(messageEnvelope.Message, cancellationToken);
    }
}

internal class MassTransitConsumer<T>(Observer<T> observer, IServiceProvider serviceProvider, Type internalHandler)
    : IConsumer<T>
    where T : class, IMessage
{
    public async Task Consume(ConsumeContext<T> context)
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(internalHandler);
        if (handler is null)
        {
            await observer.Add(null!);
            return;
        }

        await handler.InvokeMethodWithoutResultAsync("Consume", context);
        await observer.Add(context.Message);
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
