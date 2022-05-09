namespace BuildingBlocks.Abstractions.CQRS.Command;

public interface IInternalCommand : ICommand
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string Type { get; }
}
