using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Abstractions.Scheduler;

public interface IMessageScheduler
{
    Task ScheduleAsync(IMessage message, CancellationToken cancellationToken = default);
    Task ScheduleAsync(IMessage[] messages, CancellationToken cancellationToken = default);
}
