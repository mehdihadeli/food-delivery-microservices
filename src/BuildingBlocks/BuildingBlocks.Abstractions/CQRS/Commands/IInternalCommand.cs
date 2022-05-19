namespace BuildingBlocks.Abstractions.CQRS.Commands;

public interface IInternalCommand : ICommand
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string Type { get; }
}
