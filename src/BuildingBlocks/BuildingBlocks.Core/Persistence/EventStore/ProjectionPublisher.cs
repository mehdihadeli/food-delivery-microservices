using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence.EventStore.Projections;

namespace BuildingBlocks.Core.Persistence.EventStore;

public class ReadProjectionPublisher(IServiceProvider serviceProvider) : IReadProjectionPublisher
{
    public async Task PublishAsync<T>(
        IStreamEventEnvelope<T> streamEvent,
        CancellationToken cancellationToken = default
    )
        where T : class, IDomainEvent
    {
        using var scope = serviceProvider.CreateScope();
        var projections = scope.ServiceProvider.GetRequiredService<IEnumerable<IHaveReadProjection>>();
        foreach (var projection in projections)
        {
            await projection.ProjectAsync(streamEvent, cancellationToken);
        }
    }

    public Task PublishAsync(IStreamEventEnvelopeBase streamEvent, CancellationToken cancellationToken = default)
    {
        var streamData = streamEvent.Data.GetType();

        var method = typeof(IReadProjectionPublisher)
            .GetMethods()
            .First(m => m.Name == nameof(PublishAsync) && m.GetGenericArguments().Length != 0)
            .MakeGenericMethod(streamData);

        return (Task)method.Invoke(this, [streamEvent, cancellationToken])!;
    }
}
