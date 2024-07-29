using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Web.Extenions;
using BuildingBlocks.Core.Web.HeaderPropagation;
using MassTransit;

namespace BuildingBlocks.Integration.MassTransit;

public class CorrelationPublishFilter<T> : IFilter<PublishContext<T>>
    where T : class, IEventEnvelope
{
    private readonly CustomHeaderPropagationStore _headerStore;

    public CorrelationPublishFilter(CustomHeaderPropagationStore headerStore)
    {
        _headerStore = headerStore;
    }

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        var correlationId = _headerStore.GetCorrelationId();

        context.CorrelationId = correlationId;

        return next.Send(context);
    }

    public void Probe(ProbeContext context) { }
}
