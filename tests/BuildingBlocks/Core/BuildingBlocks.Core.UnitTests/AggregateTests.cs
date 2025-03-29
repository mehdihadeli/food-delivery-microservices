using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.Events.Internal;
using FluentAssertions;

namespace BuildingBlocks.Core.UnitTests;

public class AggregateTests
{
    [Fact]
    public void has_uncommitted_event_should_return_true_when_there_is_an_uncommitted_event()
    {
        var clientId = Guid.NewGuid();

        var aggregate = ShoppingCart.Create(clientId);

        aggregate.HasUncommittedDomainEvents().Should().BeTrue();
    }

    [Fact]
    public void add_domain_events_should_add_correct_events_to_uncommitted_domain_events()
    {
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var aggregate = ShoppingCart.Create(clientId);
        aggregate.AddItem(productId);
        aggregate.Confirm();

        aggregate.GetUncommittedDomainEvents().Should().HaveCount(3);

        aggregate.GetUncommittedDomainEvents().Last().Should().BeOfType<ShoppingCartConfirmed>();
    }

    [Fact]
    public void mark_events_as_committed_should_remove_events_from_uncommitted_domain_events()
    {
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var aggregate = ShoppingCart.Create(clientId);
        aggregate.AddItem(productId);
        aggregate.Confirm();

        aggregate.MarkUncommittedDomainEventAsCommitted();

        aggregate.GetUncommittedDomainEvents().Should().HaveCount(0);
    }

    [Fact]
    public void flush_uncommitted_should_return_all_uncommitted_event_and_clear_this_list()
    {
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var shoppingCard = ShoppingCart.Create(clientId);
        shoppingCard.AddItem(productId);
        shoppingCard.Confirm();

        var events = shoppingCard.GetUncommittedDomainEvents();
        events.Count.Should().Be(3);

        shoppingCard.MarkUncommittedDomainEventAsCommitted();
        events.Count.Should().Be(3);
        shoppingCard.HasUncommittedDomainEvents().Should().BeFalse();

        events.Last().Should().BeOfType<ShoppingCartConfirmed>();
        events.Last().AggregateSequenceNumber.Should().Be(2);
        ((Guid)events.Last().AggregateId).Should().Be(shoppingCard.Id);

        shoppingCard.OriginalVersion.Should().Be(0);
    }

    # region Models

    private record ShoppingCartInitialized(Guid ShoppingCartId, Guid ClientId) : DomainEvent;

    private record ProductItemAddedToShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ProductItemRemovedFromShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;

    private record ShoppingCartConfirmed(Guid ShoppingCartId, DateTime ConfirmedAt) : DomainEvent;

    private enum ShoppingCartStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 4,
    }

    private class ShoppingCart : Aggregate<Guid>
    {
        private List<Guid> _products = new();

        public Guid ClientId { get; private set; }
        public ShoppingCartStatus Status { get; private set; }
        public IReadOnlyList<Guid> Products => _products.AsReadOnly();
        public DateTime? ConfirmedAt { get; private set; }

        public static ShoppingCart Create(Guid clientId)
        {
            var shoppingCart = new ShoppingCart
            {
                ClientId = clientId,
                Id = Guid.NewGuid(),
                _products = new List<Guid>(),
                Status = ShoppingCartStatus.Pending,
            };

            shoppingCart.AddDomainEvents(new ShoppingCartInitialized(shoppingCart.Id, shoppingCart.ClientId));

            return shoppingCart;
        }

        public void AddItem(Guid productId)
        {
            _products.Add(productId);

            AddDomainEvents(new ProductItemAddedToShoppingCart(Id, productId));
        }

        public void RemoveItem(Guid productId)
        {
            _products.Remove(productId);

            AddDomainEvents(new ProductItemRemovedFromShoppingCart(Id, productId));
        }

        public void Confirm()
        {
            ConfirmedAt = DateTime.Now;
            Status = ShoppingCartStatus.Confirmed;

            AddDomainEvents(new ShoppingCartConfirmed(Id, DateTime.Now));
        }
    }

    # endregion
}
