using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using BuildingBlocks.Core.Extensions;
using Hypothesist;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Fixtures;

public static class HandlerFactory
{
    public static IMessageHandler<T> AsMessageHandler<T>(this IHypothesis<T> hypothesis) where T : class, IMessage
    {
        return new SimpleMessageConsumer<T>(hypothesis);
    }

    public static IConsumer<T> AsConsumer<T>(this IHypothesis<T> hypothesis) where T : class, IMessage
    {
        return new MassTransitSimpleMessageConsumer<T>(hypothesis);
    }

    public static IConsumer<TMessage> AsConsumer<TMessage, TConsumer>(
        this IHypothesis<TMessage> hypothesis, IServiceProvider serviceProvider)
        where TMessage : class, IMessage
        where TConsumer : IConsumer<TMessage>
    {
        return new MassTransitConsumer<TMessage>(hypothesis, serviceProvider, typeof(TConsumer));
    }

    public static BuildingBlocks.Abstractions.Messaging.MessageHandler<T> AsMessageHandlerDelegate<T>(
        this IHypothesis<T> hypothesis)
        where T : class, IMessage
    {
        return (context, cancellationToken) => hypothesis.Test(context.Message, cancellationToken);
    }

    public static IMessageHandler<TMessage> AsMessageHandler<TMessage, TMessageHandler>(
        this IHypothesis<TMessage> hypothesis, IServiceProvider serviceProvider)
        where TMessage : class, IMessage
        where TMessageHandler : IMessageHandler<TMessage>
    {
        return new MessageConsumer<TMessage>(hypothesis, serviceProvider, typeof(TMessageHandler));
    }
}

internal class MessageConsumer<T> : IMessageHandler<T>
    where T : class, IMessage
{
    private readonly IHypothesis<T> _hypothesis;
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _internalHandler;

    public MessageConsumer(IHypothesis<T> hypothesis, IServiceProvider serviceProvider, Type internalHandler)
    {
        _hypothesis = hypothesis;
        _serviceProvider = serviceProvider;
        _internalHandler = internalHandler;
    }

    public async Task HandleAsync(IConsumeContext<T> messageContext, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(_internalHandler);
        if (handler is null)
        {
            await _hypothesis.Test(null!, cancellationToken);
            return;
        }

        await handler.InvokeMethodWithoutResultAsync("HandleAsync", messageContext, cancellationToken);
        await _hypothesis.Test(messageContext.Message);
    }
}

internal class SimpleMessageConsumer<T> : IMessageHandler<T>
    where T : class, IMessage
{
    private readonly IHypothesis<T> _hypothesis;

    public SimpleMessageConsumer(IHypothesis<T> hypothesis)
    {
        _hypothesis = hypothesis;
    }

    public Task HandleAsync(IConsumeContext<T> messageContext, CancellationToken cancellationToken = default)
    {
        return _hypothesis.Test(messageContext.Message);
    }
}


internal class MassTransitConsumer<T> : IConsumer<T>
    where T : class, IMessage
{
    private readonly IHypothesis<T> _hypothesis;
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _internalHandler;

    public MassTransitConsumer(IHypothesis<T> hypothesis, IServiceProvider serviceProvider, Type internalHandler)
    {
        _hypothesis = hypothesis;
        _serviceProvider = serviceProvider;
        _internalHandler = internalHandler;
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(_internalHandler);
        if (handler is null)
        {
            await _hypothesis.Test(null!);
            return;
        }

        await handler.InvokeMethodWithoutResultAsync("Consume", context);
        await _hypothesis.Test(context.Message);
    }
}

internal class MassTransitSimpleMessageConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    private readonly IHypothesis<TMessage> _hypothesis;

    public MassTransitSimpleMessageConsumer(IHypothesis<TMessage> hypothesis)
    {
        _hypothesis = hypothesis;
    }

    public Task Consume(ConsumeContext<TMessage> context)
    {
        return _hypothesis.Test(context.Message);
    }
}
