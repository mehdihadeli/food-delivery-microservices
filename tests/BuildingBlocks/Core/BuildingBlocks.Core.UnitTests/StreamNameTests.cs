using BuildingBlocks.Core.Domain.EventSourcing;
using BuildingBlocks.Core.Persistence.EventStore;
using FluentAssertions;

namespace BuildingBlocks.Core.UnitTests;

public class StreamNameTests
{
    [Fact]
    public void should_return_correct_streamId_value_for_event_sourced_aggregate()
    {
        var id = Guid.NewGuid();
        var streamName = StreamName.For<TestAggregate, Guid>(id);
        streamName.Should().NotBeNull();
        streamName.Value.Should().NotBeNullOrEmpty();
        streamName.Value.Should().Be($"{nameof(TestAggregate)}-{id.ToString()}");
    }
}

public class TestAggregate : EventSourcedAggregate<Guid> { }
