namespace BuildingBlocks.Abstractions.Commands;

public interface ICreateCommand<out TResponse> : ICommand<TResponse>
    where TResponse : notnull;

public interface ICreateCommand : ICommand;
