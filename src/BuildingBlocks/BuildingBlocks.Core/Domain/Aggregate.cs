using System.Collections.Concurrent;
using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Core.Domain.Exceptions;

namespace BuildingBlocks.Core.Domain;

public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{
    [NonSerialized]
    private readonly ConcurrentQueue<IDomainEvent> _uncommittedDomainEvents = new();

    private const long NewAggregateVersion = 0;

    public long OriginalVersion { get; private set; } = NewAggregateVersion;

    /// <summary>
    /// Add the <paramref name="domainEvent"/> to the aggregate pending changes event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    protected void AddDomainEvents(IDomainEvent domainEvent)
    {
        if (!_uncommittedDomainEvents.Any(x => Equals(x.EventId, domainEvent.EventId)))
        {
            _uncommittedDomainEvents.Enqueue(domainEvent);
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

    public void ClearDomainEvents()
    {
        _uncommittedDomainEvents.Clear();
    }

    public IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents()
    {
        var events = _uncommittedDomainEvents.ToImmutableList();
        MarkUncommittedDomainEventAsCommitted();
        return events;
    }

    public void MarkUncommittedDomainEventAsCommitted()
    {
        _uncommittedDomainEvents.Clear();
    }

    public void CheckRule(IBusinessRule rule)
    {
        var isBroken = rule.IsBroken();
        if (isBroken)
        {
            throw new DomainException(rule.GetType(), rule.Message, rule.Status);
        }
    }

    public void CheckRule<T>(IBusinessRuleWithExceptionType<T> ruleWithExceptionType)
        where T : DomainException
    {
        var isBroken = ruleWithExceptionType.IsBroken();
        if (isBroken)
        {
            throw ruleWithExceptionType.Exception;
        }
    }
}

public abstract class Aggregate<TIdentity, TId> : Aggregate<TIdentity>
    where TIdentity : Identity<TId> { }

public abstract class Aggregate : Aggregate<AggregateId, long>, IAggregate { }
