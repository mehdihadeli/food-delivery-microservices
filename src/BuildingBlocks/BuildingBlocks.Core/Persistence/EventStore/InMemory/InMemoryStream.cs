using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Core.Persistence.EventStore.InMemory;

public class InMemoryStream(string name)
{
    private readonly List<StreamEventData> _events = new();

    public int Version { get; private set; } = -1;

    public string StreamName { get; } = name;

    public void CheckVersion(ExpectedStreamVersion expectedVersion)
    {
        if (
            (expectedVersion.Value == ExpectedStreamVersion.NoStream.Value && _events.Count != 0 == false)
            || expectedVersion.Value == ExpectedStreamVersion.Any.Value
        )
            return;
        if (expectedVersion.Value != Version)
            throw new System.Exception($"Wrong stream version. Expected {expectedVersion.Value}, actual {Version}");
    }

    public void AppendEvents(
        ExpectedStreamVersion expectedVersion,
        int globalAllPosition,
        IReadOnlyCollection<StreamEventData> events
    )
    {
        CheckVersion(expectedVersion);

        foreach (var @event in events)
        {
            var version = ++Version;
            @event.StreamId = StreamName;
            @event.Name = $"{version}@{StreamName}";
            @event.Timestamp = DateTime.Now;
            @event.EventNumber = version;
            @event.GlobalEventPosition = globalAllPosition + 1;
        }

        _events.AddRange(events);
    }

    public IEnumerable<StreamEventData> GetEvents(StreamReadPosition from, int count)
    {
        var selected = _events.SkipWhile(x => x.GlobalEventPosition < from.Value);

        if (count > 0)
            selected = selected.Take(count);

        return selected;
    }

    public IEnumerable<StreamEventData> GetEventsBackwards(int count)
    {
        var position = _events.Count - 1;

        while (count-- > 0)
        {
            yield return _events[position--];
        }
    }
}
