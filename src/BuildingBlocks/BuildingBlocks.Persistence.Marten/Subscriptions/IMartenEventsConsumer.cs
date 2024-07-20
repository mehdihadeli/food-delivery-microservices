using Marten;
using Marten.Events;

namespace BuildingBlocks.Persistence.Marten.Subscriptions;

public interface IMartenEventsConsumer
{
    Task ConsumeAsync(
        IDocumentOperations documentOperations,
        IReadOnlyList<StreamAction> streamActions,
        CancellationToken cancellationToken = default
    );
}
