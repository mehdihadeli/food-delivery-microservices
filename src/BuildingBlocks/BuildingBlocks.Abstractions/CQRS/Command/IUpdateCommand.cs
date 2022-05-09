using MediatR;

namespace BuildingBlocks.Abstractions.CQRS.Command;

public interface IUpdateCommand : IUpdateCommand<Unit>
{
}

public interface IUpdateCommand<out TResponse> : ICommand<TResponse>
    where TResponse : notnull
{
}
