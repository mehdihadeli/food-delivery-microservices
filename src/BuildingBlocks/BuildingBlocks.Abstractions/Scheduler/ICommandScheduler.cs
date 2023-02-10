using BuildingBlocks.Abstractions.CQRS.Commands;

namespace BuildingBlocks.Abstractions.Scheduler;

public interface ICommandScheduler
{
    Task ScheduleAsync(IInternalCommand command, CancellationToken cancellationToken = default);
    Task ScheduleAsync(IInternalCommand[] commands, CancellationToken cancellationToken = default);
    Task ScheduleAsync(IInternalCommand command, DateTimeOffset scheduleAt, string? description = null);
    Task ScheduleAsync(IInternalCommand[] commands, DateTimeOffset scheduleAt, string? description = null);
    Task ScheduleRecurringAsync(
        IInternalCommand command,
        string name,
        string cronExpression,
        string? description = null
    );
}
