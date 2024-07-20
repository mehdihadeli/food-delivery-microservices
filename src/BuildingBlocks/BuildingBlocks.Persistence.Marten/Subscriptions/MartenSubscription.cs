using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Persistence.Marten.Subscriptions;

public class MartenSubscription : IProjection
{
    private readonly IEnumerable<IMartenEventsConsumer> _consumers;
    private readonly ILogger<MartenSubscription> _logger;

    public MartenSubscription(IEnumerable<IMartenEventsConsumer> consumers, ILogger<MartenSubscription> logger)
    {
        _consumers = consumers;
        _logger = logger;
    }

    public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams) =>
        throw new NotImplementedException("Subscriptions should work only in the async scope");

    public async Task ApplyAsync(
        IDocumentOperations operations,
        IReadOnlyList<StreamAction> streams,
        CancellationToken cancellation
    )
    {
        try
        {
            foreach (var consumer in _consumers)
            {
                await consumer.ConsumeAsync(operations, streams, cancellation);
            }
        }
        catch (Exception exc)
        {
            _logger.LogError("Error while processing Marten Subscription: {ExceptionMessage}", exc.Message);
            throw;
        }
    }
}
