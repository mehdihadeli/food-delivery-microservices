using MediatR;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IMessage : INotification
{
    Guid MessageId { get; }
    string MessageType { get; }
    DateTime Created { get; }
}
