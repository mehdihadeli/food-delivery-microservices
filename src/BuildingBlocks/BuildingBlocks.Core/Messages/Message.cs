namespace BuildingBlocks.Core.Messages;

using BuildingBlocks.Abstractions.Messages;

public abstract record Message : IMessage
{
    public Guid MessageId => Guid.CreateVersion7();
    public DateTime Created { get; } = DateTime.Now;
}
