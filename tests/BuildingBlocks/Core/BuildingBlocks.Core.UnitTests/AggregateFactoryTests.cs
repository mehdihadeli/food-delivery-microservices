using CloudNativeKit.Core.Domain;
using CloudNativeKit.Core.Domain.EventSourcing;
using FluentAssertions;

namespace CloudNativeKit.Core.UnitTests;

public class AggregateFactoryTests
{
    [Fact]
    public void Create_ShouldReturnInstanceOfAggregateRoot()
    {
        var aggregate = AggregateFactory<ShoppingCart>.CreateAggregate();

        aggregate.Should().NotBeNull();
        aggregate.Should().BeOfType<ShoppingCart>();
    }

    private class ShoppingCart : EventSourcedAggregate<Guid>
    {
        public Guid ClientId { get; private set; }
        public IList<Guid> Products { get; private set; } = new List<Guid>();
        public DateTime? ConfirmedAt { get; private set; }
    }
}
