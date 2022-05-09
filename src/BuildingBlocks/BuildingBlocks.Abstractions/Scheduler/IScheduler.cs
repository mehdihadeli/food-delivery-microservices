namespace BuildingBlocks.Abstractions.Scheduler;

public interface IScheduler : ICommandScheduler, IMessageScheduler
{
    Task ScheduleAsync(
        ScheduleSerializedObject scheduleSerializedObject,
        DateTimeOffset scheduleAt,
        string? description = null);

    Task ScheduleAsync(ScheduleSerializedObject scheduleSerializedObject, TimeSpan delay, string? description = null);

    Task ScheduleRecurringAsync(
        ScheduleSerializedObject scheduleSerializedObject,
        string name,
        string cronExpression,
        string? description = null);
}
