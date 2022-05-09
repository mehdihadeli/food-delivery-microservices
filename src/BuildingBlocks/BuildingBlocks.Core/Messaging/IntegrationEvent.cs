using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Types;

namespace BuildingBlocks.Core.Messaging;

public record IntegrationEvent : IIntegrationEvent
{
    public Guid MessageId { get; set; }
    public string MessageType => TypeMapper.GetTypeName(GetType());
    public DateTime Created => DateTime.Now;
}
