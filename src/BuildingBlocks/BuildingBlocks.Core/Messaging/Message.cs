using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Types;

namespace BuildingBlocks.Core.Messaging;

public record Message : IMessage
{
    public Guid MessageId { get; set; }
    public string MessageType { get; } = TypeMapper.GetTypeName(typeof(Message));
    public DateTime Created { get; } = DateTime.Now;
}
