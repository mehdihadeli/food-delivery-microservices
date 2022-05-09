using BuildingBlocks.Abstractions.Scheduler;
using BuildingBlocks.Core.Extensions;
using MediatR;
using Newtonsoft.Json;

namespace BuildingBlocks.Core.CQRS;

public static class MediatRExtensions
{
    public static async Task SendScheduleObject(
        this IMediator mediator,
        ScheduleSerializedObject scheduleSerializedObject)
    {
        var type = scheduleSerializedObject.GetPayloadType();

        dynamic? req = JsonConvert.DeserializeObject(scheduleSerializedObject.Data, type);

        if (req is null)
        {
            return;
        }

        await mediator.Send(req);
    }
}
