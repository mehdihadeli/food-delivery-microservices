using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Domain.EventSourcing;
using FluentAssertions;

namespace BuildingBlocks.Core.UnitTests;

public class EventSourcedAggregateTests
{
    [Fact]
    public void when_should_get_the_current_state_from_the_events_without_changing_versions()
    {
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var shoppingCartInitialized = new ShoppingCartInitialized(Guid.NewGuid(), clientId);
        var productItemAddedToShoppingCart = new ProductItemAddedToShoppingCart(id, productId);
        var shoppingCartConfirmed = new ShoppingCartConfirmed(id, DateTime.Now);

        // 1. Get all events and sort them in the order of appearance
        var events = new object[]
        {
            shoppingCartInitialized,
            productItemAddedToShoppingCart,
            productItemAddedToShoppingCart,
            shoppingCartConfirmed
        };

        // 2. Construct empty Invoice object
        var shoppingCart = AggregateFactory<ShoppingCart>.CreateAggregate();

        // 3. Apply each event on the entity.
        foreach (var @event in events)
        {
            shoppingCart.When(@event);
        }

        shoppingCart.Id.Should().Be(shoppingCartInitialized.ShoppingCartId);
        shoppingCart.Products.Count.Should().Be(2);
        shoppingCart.ConfirmedAt.Should().Be(shoppingCartConfirmed.ConfirmedAt);
        shoppingCart.ClientId.Should().Be(shoppingCartInitialized.ClientId);

        // Versions should not be touched on this action
        shoppingCart.CurrentVersion.Should().Be(-1);
        shoppingCart.OriginalVersion.Should().Be(-1);
    }

    [Fact]
    public void apply_event_should_update_state_and_add_event_to_uncommitted_events_with_increase_current_version()
    {
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var shoppingCard = ShoppingCart.Create(clientId);
        shoppingCard.AddItem(productId);

        shoppingCard.Products.Count.Should().Be(1);
        shoppingCard.ClientId.Should().Be(clientId);

        shoppingCard.OriginalVersion.Should().Be(-1);
        shoppingCard.CurrentVersion.Should().Be(1);

        var uncommittedEvents = shoppingCard.GetUncommittedDomainEvents();

        uncommittedEvents.Count.Should().Be(2);
        uncommittedEvents.Last().Should().BeOfType<ProductItemAddedToShoppingCart>();
        ((Guid)uncommittedEvents.Last().AggregateId).Should().Be(shoppingCard.Id);
        uncommittedEvents.Last().AggregateSequenceNumber.Should().Be(1);
    }

    [Fact]
    public void fold_should_apply_events_to_the_aggregate_state_and_increase_current_and_original_version()
    {
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var shoppingCartInitialized = new ShoppingCartInitialized(Guid.NewGuid(), clientId);
        var productItemAddedToShoppingCart = new ProductItemAddedToShoppingCart(id, productId);
        var shoppingCartConfirmed = new ShoppingCartConfirmed(id, DateTime.Now);

        // 1. Get all events and sort them in the order of appearance
        var events = new object[]
        {
            shoppingCartInitialized,
            productItemAddedToShoppingCart,
            productItemAddedToShoppingCart,
            shoppingCartConfirmed
        };

        // 2. Construct empty Invoice object
        var shoppingCart = AggregateFactory<ShoppingCart>.CreateAggregate();

        // 3. Apply each event on the entity.
        foreach (var @event in events)
        {
            shoppingCart.Fold(@event);
        }

        shoppingCart.Id.Should().Be(shoppingCartInitialized.ShoppingCartId);
        shoppingCart.Products.Count.Should().Be(2);
        shoppingCart.ConfirmedAt.Should().Be(shoppingCartConfirmed.ConfirmedAt);
        shoppingCart.ClientId.Should().Be(shoppingCartInitialized.ClientId);

        // Versions should be touched on this action
        shoppingCart.CurrentVersion.Should().Be(3);
        shoppingCart.OriginalVersion.Should().Be(3);
    }

    [Fact]
    public void flush_uncommitted_should_return_all_uncommitted_event_and_clear_this_list()
    {
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var shoppingCard = ShoppingCart.Create(clientId);
        shoppingCard.AddItem(productId);

        var uncommittedEvents = shoppingCard.GetUncommittedDomainEvents();
        uncommittedEvents.Count.Should().Be(2);

        uncommittedEvents.Last().Should().BeOfType<ProductItemAddedToShoppingCart>();

        shoppingCard.MarkUncommittedDomainEventAsCommitted();

        shoppingCard.GetUncommittedDomainEvents().Count.Should().Be(0);
        shoppingCard.CurrentVersion.Should().Be(shoppingCard.OriginalVersion);
    }

    private record ShoppingCartInitialized(Guid ShoppingCartId, Guid ClientId) : DomainEvent;

    private record ProductItemAddedToShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ProductItemRemovedFromShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ShoppingCartConfirmed(Guid ShoppingCartId, DateTime ConfirmedAt) : DomainEvent;

    private enum ShoppingCartStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 4
    }

    private class ShoppingCart : EventSourcedAggregate<Guid>
    {
        private List<Guid> _products = new();

        public Guid ClientId { get; private set; }
        public ShoppingCartStatus Status { get; private set; }
        public IReadOnlyList<Guid> Products => _products.AsReadOnly();
        public DateTime? ConfirmedAt { get; private set; }

        public static ShoppingCart Create(Guid clientId)
        {
            var shoppingCart = new ShoppingCart();

            shoppingCart.ApplyEvent(new ShoppingCartInitialized(Guid.NewGuid(), clientId));

            return shoppingCart;
        }

        public void AddItem(Guid productId)
        {
            ApplyEvent(new ProductItemAddedToShoppingCart(Id, productId));
        }

        public void RemoveItem(Guid productId)
        {
            ApplyEvent(new ProductItemRemovedFromShoppingCart(Id, productId));
        }

        public void Confirm()
        {
            ApplyEvent(new ShoppingCartConfirmed(Id, DateTime.Now));
        }

        internal void Apply(ShoppingCartInitialized @event)
        {
            Id = @event.ShoppingCartId;
            ClientId = @event.ClientId;
            Status = ShoppingCartStatus.Pending;
            _products = new List<Guid>();
        }

        internal void Apply(ProductItemAddedToShoppingCart @event)
        {
            _products.Add(@event.ProductId);
        }

        internal void Apply(ProductItemRemovedFromShoppingCart @event)
        {
            _products.Remove(@event.ProductId);
        }

        internal void Apply(ShoppingCartConfirmed @event)
        {
            ConfirmedAt = @event.ConfirmedAt;
            Status = ShoppingCartStatus.Confirmed;
        }
    }
}
