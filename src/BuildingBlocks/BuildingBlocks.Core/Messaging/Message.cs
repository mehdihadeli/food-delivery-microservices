using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Core.Messaging;

public abstract record Message : IMessage
{
    public Guid MessageId => Guid.NewGuid();
    public DateTime Created { get; } = DateTime.Now;
}
