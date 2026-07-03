using BuildingBlocks.Core.Web.Extensions;
using Microsoft.AspNetCore.HeaderPropagation;

namespace BuildingBlocks.Core.Messages;

using BuildingBlocks.Abstractions.Messages;

public class MessageMetadataAccessor(HeaderPropagationValues headerPropagationValues) : IMessageMetadataAccessor
{
    public Guid GetCorrelationId()
    {
        var cid = headerPropagationValues.GetCorrelationId();

        if (cid is not null)
        {
            return (Guid)cid;
        }

        var correlationId = Guid.CreateVersion7();
        headerPropagationValues.AddCorrelationId(correlationId);

        return correlationId;
    }

    public Guid? GetCautionId()
    {
        var causationId = headerPropagationValues.GetCausationId();

        if (causationId is not null)
        {
            return (Guid)causationId;
        }

        return null;
    }
}
