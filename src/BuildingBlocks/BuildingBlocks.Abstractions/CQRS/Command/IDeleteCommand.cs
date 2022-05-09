namespace BuildingBlocks.Abstractions.CQRS.Command;

public interface IDeleteCommand<TId, out TResponse> : ICommand<TResponse>
    where TId : struct
    where TResponse : notnull
{
    public TId Id { get; init; }
}

public interface IDeleteCommand<TId> : ICommand
    where TId : struct
{
    public TId Id { get; init; }
}
