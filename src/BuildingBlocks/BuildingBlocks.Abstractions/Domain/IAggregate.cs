namespace BuildingBlocks.Abstractions.Domain;

public interface IAggregate<out TId> : IEntity<TId>, IAggregateBase;

public interface IAggregate<out TIdentity, TId> : IAggregate<TIdentity>
    where TIdentity : Identity<TId>;

public interface IAggregate : IAggregate<AggregateId, long>;
