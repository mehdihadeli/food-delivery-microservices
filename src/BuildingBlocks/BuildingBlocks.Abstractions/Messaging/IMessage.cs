using MediatR;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IMessage : INotification
{
    Guid MessageId { get; set; }
    string MessageType { get; }
    DateTime Created { get; }
}
