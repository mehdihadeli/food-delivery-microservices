using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Domain;

public interface IAggregateBase
{
    /// <summary>
    /// Add domain event to list of uncommited domain events.
    /// </summary>
    /// <param name="domainEvent"></param>
    void AddDomainEvents(IDomainEvent domainEvent);

    /// <summary>
    ///     Does the aggregate have change that have not been committed to storage.
    /// </summary>
    /// <returns></returns>
    public bool HasUncommittedDomainEvents();

    /// <summary>
    /// Gets a list of uncommitted events for this aggregate and mark them as commited.
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents();

    /// <summary>
    ///     Gets a list of uncommitted events for this aggregate.
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<IDomainEvent> GetUncommittedDomainEvents();

    /// <summary>
    ///     Remove all domain events.
    /// </summary>
    void ClearDomainEvents();

    /// <summary>
    ///     Check specific rule for aggregate and throw an exception if rule is not satisfied.
    /// </summary>
    /// <param name="rule"></param>
    void CheckRule(IBusinessRule rule);
}
