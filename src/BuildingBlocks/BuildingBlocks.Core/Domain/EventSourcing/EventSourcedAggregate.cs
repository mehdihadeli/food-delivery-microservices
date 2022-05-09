using System.Collections.Concurrent;
using System.Collections.Immutable;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Domain.EventSourcing;

public abstract class EventSourcedAggregate<TId> : Entity<TId>, IEventSourcedAggregate<TId>
{
    [NonSerialized]
    private readonly ConcurrentQueue<IDomainEvent> _uncommittedDomainEvents = new();

    // -1: No Stream
    private const long NewAggregateVersion = -1;

    public long OriginalVersion { get; private set; } = NewAggregateVersion;

    public long CurrentVersion { get; private set; } = NewAggregateVersion;

    /// <summary>
    /// Applies a new event to the aggregate state, adds the event to the list of pending changes,
    /// and increases the `CurrentVersion` property and `LastCommittedVersion` will be unchanged.
    /// </summary>
    /// <typeparam name="TDomainEvent">Type of domain event.</typeparam>
    /// <param name="domainEvent"></param>
    /// <param name="isNew"></param>
    protected virtual void ApplyEvent<TDomainEvent>(TDomainEvent domainEvent, bool isNew = true)
        where TDomainEvent : IDomainEvent
    {
        if (isNew)
        {
            AddDomainEvents(domainEvent);
        }

        When(domainEvent);
        CurrentVersion++;
    }

    public void When(object @event)
    {
        if (GetType().HasAggregateApplyMethod(@event.GetType()))
        {
            this.InvokeMethod("Apply", @event);
        }
        else
        {
            throw new AggregateException($"Can't find 'Apply' method for domain event: '{@event.GetType().Name}'");
        }
    }

    public void Fold(object @event)
    {
        When(@event);
        OriginalVersion++;
        CurrentVersion++;
    }

    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        history.ToList().ForEach(Fold);
    }

    /// <summary>
    /// Add the <paramref name="domainEvent"/> to the aggregate pending changes event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    protected void AddDomainEvents(IDomainEvent domainEvent)
    {
        if (!_uncommittedDomainEvents.Any(x => Equals(x.EventId, domainEvent.EventId)))
        {
            IDomainEvent eventWithAggregate = domainEvent.WithAggregate(Id, CurrentVersion + 1);
            _uncommittedDomainEvents.Enqueue(eventWithAggregate);
        }
    }

    public bool HasUncommittedDomainEvents()
    {
        return _uncommittedDomainEvents.Any();
    }

    public IReadOnlyList<IDomainEvent> GetUncommittedDomainEvents()
    {
        return _uncommittedDomainEvents.ToImmutableList();
    }

    public void MarkUncommittedDomainEventAsCommitted()
    {
        _uncommittedDomainEvents.Clear();

        OriginalVersion = CurrentVersion;
    }

    public void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }

}
