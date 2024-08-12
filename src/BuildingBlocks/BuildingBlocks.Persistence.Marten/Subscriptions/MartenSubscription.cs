using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Persistence.Marten.Subscriptions;

public class MartenSubscription(IEnumerable<IMartenEventsConsumer> consumers, ILogger<MartenSubscription> logger)
    : IProjection
{
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
            foreach (var consumer in consumers)
            {
                await consumer.ConsumeAsync(operations, streams, cancellation);
            }
        }
        catch (Exception exc)
        {
            logger.LogError("Error while processing Marten Subscription: {ExceptionMessage}", exc.Message);
            throw;
        }
    }
}
