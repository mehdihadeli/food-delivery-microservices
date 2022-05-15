using MediatR;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IMessage : INotification
{
    Guid MessageId { get; }
    DateTime Created { get; }
}
