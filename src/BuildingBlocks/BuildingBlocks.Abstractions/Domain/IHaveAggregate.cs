using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Domain;

public interface IHaveAggregate : IHaveDomainEvents, IHaveAggregateVersion
{
    /// <summary>
    ///     Check specific rule for aggregate and throw an exception if rule is not satisfied.
    /// </summary>
    /// <param name="rule"></param>
    void CheckRule(IBusinessRule rule);
}
