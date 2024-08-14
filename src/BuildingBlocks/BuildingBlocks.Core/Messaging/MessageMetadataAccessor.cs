using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Web.HeaderPropagation;
using BuildingBlocks.Core.Web.HeaderPropagation.Extensions;
using MassTransit;

namespace BuildingBlocks.Core.Messaging;

public class MessageMetadataAccessor(HeaderPropagationStore headerPropagationStore) : IMessageMetadataAccessor
{
    public Guid GetCorrelationId()
    {
        var cid = headerPropagationStore.GetCorrelationId();

        if (cid is not null)
            return (Guid)cid;

        var correlationId = NewId.NextGuid();
        headerPropagationStore.AddCorrelationId(correlationId);

        return correlationId;
    }

    public Guid? GetCautionId()
    {
        var causationId = headerPropagationStore.GetCausationId();

        if (causationId is not null)
            return (Guid)causationId;

        return null;
    }
}
