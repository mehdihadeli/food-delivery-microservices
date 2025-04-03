namespace BuildingBlocks.Core.Messages;

using BuildingBlocks.Abstractions.Messages;
using MassTransit;

public abstract record Message : IMessage
{
    public Guid MessageId => NewId.NextGuid();
    public DateTime Created { get; } = DateTime.Now;
}
