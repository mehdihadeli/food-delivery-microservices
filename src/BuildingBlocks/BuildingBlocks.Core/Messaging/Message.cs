using BuildingBlocks.Abstractions.Messaging;
using MassTransit;

namespace BuildingBlocks.Core.Messaging;

public abstract record Message : IMessage
{
    public Guid MessageId => NewId.NextGuid();
    public DateTime Created { get; } = DateTime.Now;
}
