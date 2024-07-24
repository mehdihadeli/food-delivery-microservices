using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messaging;

public delegate Task MessageHandler<in TMessage>(
    IEventEnvelope<TMessage> context,
    CancellationToken cancellationToken = default
)
    where TMessage : class, IMessage;

public delegate Task<Acknowledgement> MessageHandlerAck<in TMessage>(
    IEventEnvelope<TMessage> context,
    CancellationToken cancellationToken = default
)
    where TMessage : class, IMessage;
