using BuildingBlocks.Abstractions.CQRS.Events.Internal;

namespace BuildingBlocks.Abstractions.CQRS.Commands;

public interface IInternalCommandMapper
{
    IReadOnlyList<IInternalCommand?>? MapToInternalCommands(IReadOnlyList<IDomainEvent> domainEvents);
    IInternalCommand? MapToInternalCommand(IDomainEvent domainEvent);
}
