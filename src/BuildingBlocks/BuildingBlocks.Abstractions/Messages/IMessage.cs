namespace BuildingBlocks.Abstractions.Messages;

public interface IMessage
{
    Guid MessageId { get; }
    DateTime Created { get; }
}
