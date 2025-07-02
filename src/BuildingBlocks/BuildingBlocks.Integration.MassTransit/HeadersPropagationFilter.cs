using BuildingBlocks.Core.Web.Extensions;
using MassTransit;
using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.Integration.MassTransit;

public class HeadersPropagationFilter<T>(HeaderPropagationValues headerPropagationValues) : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        InitializeHeaderPropagation(headerPropagationValues);

        if (headerPropagationValues.Headers is not null)
        {
            if (context.Headers.TryGetHeader(Core.Messages.MessageHeaders.CorrelationId, out object? correlationId))
            {
                headerPropagationValues.AddCorrelationId(Guid.Parse(correlationId.ToString()));
            }

            if (context.Headers.TryGetHeader(Core.Messages.MessageHeaders.CausationId, out object? causationId))
            {
                headerPropagationValues.AddCausationId(Guid.Parse(causationId.ToString()));
            }
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("headersPropagationFilter");
    }

    private static void InitializeHeaderPropagation(HeaderPropagationValues headerPropagationValues)
    {
        headerPropagationValues.Headers ??= new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
    }
}
