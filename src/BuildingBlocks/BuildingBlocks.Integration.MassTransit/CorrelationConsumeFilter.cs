using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Core.Web.HeaderPropagation;
using BuildingBlocks.Core.Web.HeaderPropagation.Extensions;
using MassTransit;

namespace BuildingBlocks.Integration.MassTransit;

public class CorrelationConsumeFilter<T>(HeaderPropagationStore headerPropagationStore) : IFilter<ConsumeContext<T>>
    where T : class
{
    public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationIdHeader = context.CorrelationId;

        if (!correlationIdHeader.HasValue)
            return next.Send(context);

        var correlationId = correlationIdHeader.Value;
        headerPropagationStore.AddCorrelationId(correlationId);
        headerPropagationStore.AddCausationId((Guid)context.MessageId!);

        return next.Send(context);
    }

    public void Probe(ProbeContext context) { }
}
